using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using CCLLC.Core;
using CCLLC.CDS.Sdk;

namespace CCLLC.CDS.FieldEncryption
{
    public class EncryptedFieldService : IEncryptedFieldService
    {
        private const string DECRYPT_FIELDS_VARIABLE_NAME = "CCLLC.EncryptedFieldService.DecryptColumns";

        public enum MaskingInstruction { Unmask, PartialMask, FullMask }

        #region Constructor Properties

        private IEncryptedFieldAccessLoggerFactory AccessLoggerFactory { get; }
        private IServiceConfiguration Configuration { get; }        
        private IEncryptionServiceFactory EncryptionServiceFactory { get; }        
        private ICDSPluginExecutionContext ExecutionContext { get; }
        private string RecordType { get; }
        private IUserRoleEvaluator RoleEvaluator { get; }

        #endregion 

        private IEncryptedFieldAccessLogger _accessLogger;
        private IEncryptedFieldAccessLogger AccessLogger
        {
            get
            {
                if (AccessLoggerFactory is null) return null;

                if (_accessLogger is null)
                {
                    _accessLogger = AccessLoggerFactory.Create(ExecutionContext);
                }

                return _accessLogger;
            }
        }

        private IEncryptionService _encryptionService;
        private IEncryptionService EncryptionService
        {
            get
            {
                if (_encryptionService is null)
                {
                    _encryptionService = this.EncryptionServiceFactory.Create(ExecutionContext);
                }

                return _encryptionService;
            }
        }

        internal EncryptedFieldService(ICDSPluginExecutionContext executionContext, string recordType, IServiceConfiguration configuration, IEncryptionServiceFactory encryptionServiceFactory, IUserRoleEvaluator roleEvaluator, IEncryptedFieldAccessLoggerFactory accessLoggerFactory)
        {
            this.ExecutionContext = executionContext ?? throw new ArgumentNullException("executionContext");
            this.RecordType = recordType ?? throw new ArgumentNullException("recordType");
            this.Configuration = configuration ?? throw new ArgumentNullException("configuration");
            this.EncryptionServiceFactory = encryptionServiceFactory ?? throw new ArgumentNullException("encryptionServiceFactory");
            this.RoleEvaluator = roleEvaluator ?? throw new ArgumentNullException("roleEvaluator");
            this.AccessLoggerFactory = accessLoggerFactory;
        }

      

        public void EncryptFields(ref Entity target)
        {
            ThrowErrorIfNotPreOp("Encrypt operations must be done in the PreOp pipeline stage.");
            
            if (!IsValid()) return;

            var recordConfig = this.Configuration.RecordConfiguration(RecordType);

            foreach (var fieldConfig in recordConfig.ConfiguredFields)
            {
                var key = fieldConfig.FieldName;

                if (target.Contains(key) && target[key] is string && target[key] != null)
                {
                    var fieldValue = target.GetValue<string>(key);
                    fieldValue = CreatePreparedFieldValue(fieldConfig, fieldValue);

                    if (fieldValue.Length > 0)
                    {
                        if (!IsMatch(fieldConfig, fieldValue))
                        {
                            throw new Exception(fieldConfig.InvalidMatchMessage);
                        }

                        target[key] = EncryptionService.Encrypt(fieldValue, null);
                    }                    
                }               
            }
        }

        public void GenerateEncryptedFieldQuery(ref QueryExpression queryExpression)
        {
            ThrowErrorIfNotPreOp("Query modification must be done in the PreOp pipeline stage.");

            if (!IsValid() || this.Configuration.EncryptedSearchTrigger is null) return;
            

            var searchValue = GetEncryptedSearchString(queryExpression.Criteria, this.Configuration.EncryptedSearchTrigger);
            if (searchValue is null) return;
           

            var recordConfig = this.Configuration.RecordConfiguration(RecordType);

            FilterExpression encryptedSearchFilter = null;

            foreach (var fieldConfig in recordConfig.ConfiguredFields)
            {
                var fieldSearchValue = CreatePreparedFieldValue(fieldConfig, searchValue);

                if (IsMatch(fieldConfig, fieldSearchValue)
                    && RoleEvaluator.IsAssignedRole(ExecutionContext, ExecutionContext.InitiatingUserId, fieldConfig.SearchRoles))
                {
                    encryptedSearchFilter = GenerateEncryptedSearchFilterIfNull(encryptedSearchFilter);

                    fieldSearchValue = EncryptionService.Encrypt(fieldSearchValue, null);

                    // the fieldFilter sets up a condition where the field name of encrypted field
                    // must equal the encrypted search value. An additional condition to only return 
                    // records if active is added unless the field configuration allows inactive record
                    // search.
                    var fieldFilter = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions = {
                                new ConditionExpression {
                                    AttributeName = fieldConfig.FieldName,
                                    Operator = ConditionOperator.Equal,
                                    Values = { fieldSearchValue }
                                }
                            }
                    };

                    if (!fieldConfig.AllowInactiveRecordSearch)
                    {
                        fieldFilter.AddCondition(new ConditionExpression
                        {
                            AttributeName = "statecode",
                            Operator = ConditionOperator.Equal,
                            Values = { 0 }
                        });
                    }

                    encryptedSearchFilter.AddFilter(fieldFilter);
                }
            }
           
            if (encryptedSearchFilter != null)
            {
                queryExpression.Criteria = encryptedSearchFilter;
            }
        }

        public void PrepareForDecryption(ref ColumnSet columnSet)
        {
            ThrowErrorIfNotPreOp("Decryption prep must be in the PreOp pipeline stage.");

            if (!IsValid()) return;

            var fieldsToProcess = new Dictionary<string, MaskingInstruction>();
            var fieldsToRemove = new List<string>();

            var recordConfig = this.Configuration.RecordConfiguration(RecordType);

            foreach (var fieldConfig in recordConfig.ConfiguredFields)
            {
                var unmaskTrigger = fieldConfig.UnmaskTriggerAttributeName ?? Configuration.GlobalUnmaskTriggerAttributeName;

                var fieldExists = columnSet.AllColumns || columnSet.Columns.Contains(fieldConfig.FieldName);
                var fieldDecryptRequested = !string.IsNullOrEmpty(fieldConfig.UnmaskTriggerAttributeName)
                    && (unmaskTrigger == "*" || columnSet.AllColumns || columnSet.Columns.Contains(unmaskTrigger));

                if (fieldExists && fieldDecryptRequested )
                {
                    var maskingInstruction = GetMaskingInstruction(fieldConfig);

                    fieldsToProcess.Add(fieldConfig.FieldName, maskingInstruction);

                    if (unmaskTrigger != "*" && !columnSet.AllColumns && !fieldsToRemove.Contains(unmaskTrigger))
                    {
                        fieldsToRemove.Add(unmaskTrigger);
                    }
                }
            }

            RemoveTriggerAttributes(ref columnSet, fieldsToRemove);
           

            PutFieldsToProcessInSharedVariables(fieldsToProcess);
        }

        public void DecryptFields(ref Entity target)
        {
            ThrowErrorIfNotPostOp("Decryption operations must be called in the PostOp pipeline stage.");

            if (!IsValid()) return;

            var fieldsToProcess = GetFieldsToProcessFromSharedVariables();

            if (fieldsToProcess is null) return;

            DecryptEntityFields(target, fieldsToProcess);
        }

        public void DecryptFields(ref EntityCollection targetCollection)
        {
            ThrowErrorIfNotPostOp("Decryption operations must be called in the PostOp pipeline stage.");

            if (!IsValid()) return;

            var fieldsToProcess = GetFieldsToProcessFromSharedVariables();

            if (fieldsToProcess is null) return;

            foreach(var target in targetCollection.Entities)
            {
                DecryptEntityFields(target, fieldsToProcess);
            }
        }


        /// <summary>
        /// Evaluates the field configuration encryption data and returns true if the data contains configuration
        /// for the desired record type and at least on field is configured for encryption.
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            return Configuration.RecordConfiguration(this.RecordType)?.ConfiguredFields?.Count >= 1;
        }

        private void ThrowErrorIfNotPreOp(string msg, params object[] args)
        {
            if (ExecutionContext.Stage != ePluginStage.PreOperation) throw new Exception(string.Format(msg, args));
        }

        private void ThrowErrorIfNotPostOp(string msg, params object[] args)
        {
            if (ExecutionContext.Stage != ePluginStage.PostOperation) throw new Exception(string.Format(msg, args));
        }

        /// <summary>
        /// Logs access to encrypted data if a logger is available.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="fieldName"></param>
        /// <param name="accessType"></param>
        private void LogAccess(Guid recordId, string fieldName, AccessType accessType)
        {            
            AccessLogger?.LogAccess(
                new RecordPointer<Guid>(this.RecordType, recordId),
                new RecordPointer<Guid>("systemuser", ExecutionContext.InitiatingUserId),
                fieldName,
                accessType);
        }

        private void RemoveTriggerAttributes(ref ColumnSet columnSet, IList<string> fieldsToRemove)
        {
            if (columnSet.AllColumns) return;
            if (fieldsToRemove.Count <= 0) return ;
              
            foreach(var fieldName in fieldsToRemove)
            {
                if (columnSet.Columns.Contains(fieldName))
                {
                    columnSet.Columns.Remove(fieldName);
                }
            }
            
        }

        /// <summary>
        /// Adds the field processing instructions to the plugin shared variables collection for use by the Decrypt 
        /// methods on a plugin post op call.
        /// </summary>
        /// <param name="fieldsToProcess"></param>
        private void PutFieldsToProcessInSharedVariables(Dictionary<string,MaskingInstruction> fieldsToProcess)
        {
            if (fieldsToProcess.Count > 0)
            {
                ExecutionContext.SharedVariables.Add(DECRYPT_FIELDS_VARIABLE_NAME, fieldsToProcess);
            }
        }

        /// <summary>
        /// Retrieves field processing instructions from the plugin shared variables collection if available.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string,MaskingInstruction> GetFieldsToProcessFromSharedVariables()
        {
            return (ExecutionContext.SharedVariables.ContainsKey(DECRYPT_FIELDS_VARIABLE_NAME))
                ? (Dictionary<string, MaskingInstruction>)ExecutionContext.SharedVariables[DECRYPT_FIELDS_VARIABLE_NAME] : null;
        }

        /// <summary>
        /// Uses provided field processing instructions to modify entity fields by decrypting and masking the
        /// field data.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldsToProcess"></param>
        private void DecryptEntityFields(Entity entity, Dictionary<string,MaskingInstruction> fieldsToProcess)
        {
            var recordConfig = this.Configuration.RecordConfiguration(RecordType);

            foreach (var fieldName in fieldsToProcess.Keys)
            {
                if (!entity.Contains(fieldName) || entity[fieldName] is null) break;

                var fieldConfig = recordConfig.ConfiguredField(fieldName);

                switch (fieldsToProcess[fieldName])
                {
                    case MaskingInstruction.FullMask:
                        entity[fieldName] = fieldConfig.FullyMaskedFormat;
                        break;

                    case MaskingInstruction.PartialMask:
                        entity[fieldName] = FormatField(
                             EncryptionService.Decrypt(entity.GetAttributeValue<string>(fieldName), null),
                             fieldConfig.PartiallyMaskedPattern,
                             fieldConfig.PartiallyMaskedFormat);
                        LogAccess(entity.Id, fieldName, AccessType.MaskedAccess);
                        break;

                    case MaskingInstruction.Unmask:
                        entity[fieldName] = FormatField(
                            EncryptionService.Decrypt(entity.GetAttributeValue<string>(fieldName), null),
                            fieldConfig.UnmaskedPattern,
                            fieldConfig.UnmaskedFormat);
                        LogAccess(entity.Id, fieldName, AccessType.FullAccess);
                        break;
                }
            }
        }
        

        /// <summary>
        /// Format the field value using the supplied pattern to split apart the input string 
        /// value, and the supplied format to put it back together with any required formatting.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private string FormatField(string value, string pattern, string format)
        {
            if (string.IsNullOrEmpty(value) 
                || string.IsNullOrEmpty(pattern) 
                || string.IsNullOrEmpty(format)) return value;

            Regex re = new Regex(pattern);
            return re.Replace(value, format);
        }

        /// <summary>
        /// Evaluate value against match pattern in the supplied field configuration and return
        /// true if they match.
        /// </summary>
        /// <param name="fieldConfig"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsMatch(IFieldConfiguration fieldConfig, string value)
        {
            return !string.IsNullOrEmpty(fieldConfig.MatchPattern)
                && (fieldConfig.MatchPattern == "*" || Regex.IsMatch(value, fieldConfig.MatchPattern, RegexOptions.IgnoreCase));
        }


       

        private FilterExpression GenerateEncryptedSearchFilterIfNull(FilterExpression encryptedSearchFilter)
        {

            if (encryptedSearchFilter is null)
            {
                return new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or,
                    IsQuickFindFilter = false
                };
            }

            return encryptedSearchFilter;
        }


        /// <summary>
        /// Recursively search through conditions in the filter and sub-filters to find the first occurrence of
        /// a search condition that looks like matches the encrypted search format (a 'Like' operator for a string
        /// where the value starts with the encrypted search tag and ends with a wild card character.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="encryptedSearchTag"></param>
        /// <returns></returns>
        private string GetEncryptedSearchString(FilterExpression filter, string encryptedSearchTag)
        {
            if (filter == null)
            {
                return null;
            }

            // check for any search strings in the filter conditions that match the encrypted search
            // pattern and return the search value without the leading encrypted search tag and without
            // the trailing wild card character.
            foreach (ConditionExpression condition in filter.Conditions)
            {
                if (condition.Operator == ConditionOperator.Like)
                {
                    foreach (object val in condition.Values)
                    {
                        if ((val is string) &&
                            ((string)val).StartsWith(encryptedSearchTag) &&
                            ((string)val).EndsWith("%"))
                        {
                            var returnValue = (string)val;

                            // remove leading encrypted search prefix which could be multiple characters and remove the 
                            // trailing % wild card character. If the original search contained any * characters the CDS
                            // platform changed them to % so change those back now.
                            returnValue = returnValue
                                .Substring(encryptedSearchTag.Length, returnValue.Length - encryptedSearchTag.Length - 1)
                                .Trim()
                                .Replace("%", "*");

                            return returnValue;
                        }
                    }
                }
            }

            // did not find the search criteria in the filter so examine any child filters.
            foreach (FilterExpression subfilter in filter.Filters)
            {
                var subFilterValue = GetEncryptedSearchString(subfilter, encryptedSearchTag);
                if (subFilterValue != null)
                {
                    return subFilterValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Prepare field value by applying any RegEx prep patterns to remove unwanted formatting etc. Preparation
        /// instructions are contained in the <see cref="IFieldConfiguration"/> for the specific field.
        /// </summary>
        /// <param name="fieldConfig"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        private string CreatePreparedFieldValue(IFieldConfiguration fieldConfig, string searchValue)
        {
            if (fieldConfig.DisableFieldPrep) return searchValue;

            var prepPattern = fieldConfig.FieldPrepPattern ?? Configuration.GlobalFieldPrepPattern;

            if (string.IsNullOrEmpty(prepPattern)) return searchValue;

            return Regex.Replace(searchValue, prepPattern, "");
        }

        private MaskingInstruction GetMaskingInstruction(IFieldConfiguration fieldConfig)
        {
            if (RoleEvaluator.IsAssignedRole(ExecutionContext, ExecutionContext.InitiatingUserId, fieldConfig.UnmaskedViewRoles))
                return MaskingInstruction.Unmask;

            if (RoleEvaluator.IsAssignedRole(ExecutionContext, ExecutionContext.InitiatingUserId, fieldConfig.PartiallyMaskedViewRoles))
                return MaskingInstruction.PartialMask;

            return MaskingInstruction.FullMask;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        
        public void Dispose()
        {           
            Dispose(true);            
        }

       


        #endregion

    }
}
