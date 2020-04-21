using System;
using Microsoft.Xrm.Sdk.Query;

namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.CDS.Sdk;

    public class EncryptedFieldServiceFactory : IEncryptedFieldServiceFactory
    {
        private const string CACHE_KEY = "CCLLC.CDS.EncryptedFieldServiceFactory";

        private IEncryptionServiceFactory EncryptionServiceFactory { get; }
        private IEncryptedFieldSettingsFactory SettingsFactory { get; }
        private IUserRoleEvaluator RoleEvaluator { get; }
        private IEncryptedFieldAccessLoggerFactory AccessLoggerFactory { get; }

        public EncryptedFieldServiceFactory(IEncryptedFieldSettingsFactory settingsFactory, IEncryptionServiceFactory encryptionServiceFactory, IUserRoleEvaluator roleEvaluator, IEncryptedFieldAccessLoggerFactory accessLoggerFactory) 
        {
            this.SettingsFactory = settingsFactory ?? throw new ArgumentNullException("settingsFactory");
            this.EncryptionServiceFactory = encryptionServiceFactory ?? throw new ArgumentNullException("encryptionServiceFactory");
            this.RoleEvaluator = roleEvaluator ?? throw new ArgumentNullException("roleEvaluator");
            this.AccessLoggerFactory = accessLoggerFactory;
        }

        public IEncryptedFieldService Create(ICDSPluginExecutionContext executionContext, string recordType, bool disableCache = false)
        {
            var config = LoadConfiguration(executionContext, disableCache);
            return new EncryptedFieldService(executionContext, recordType, config, this.EncryptionServiceFactory, this.RoleEvaluator, this.AccessLoggerFactory);            
        }

        private IServiceConfiguration LoadConfiguration(ICDSExecutionContext executionContext, bool disableCache)
        {
            var settings = SettingsFactory.Create(executionContext);

            if(!disableCache && executionContext.Cache.Exists(CACHE_KEY))
            {
                return executionContext.Cache.Get<IServiceConfiguration>(CACHE_KEY);
            }

            IServiceConfiguration config = new ServiceConfiguration();

            //get all XML data configuration files located in the virtual configuration path.
            var qry = new QueryExpression
            {
                EntityName = "webresource",
                ColumnSet = new ColumnSet("name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "name",
                            Operator = ConditionOperator.Like,
                            Values = { settings.ConfigurationDataPath }
                        }
                    }
                }
            };

            var xmlDataResources = executionContext.OrganizationService.RetrieveMultiple(qry).Entities;

            executionContext.Trace(string.Format("Retrieved {0} XML Data resource records.", xmlDataResources.Count));

            // Process each named XML configuration resource to build out the field level configuration service configuration.
            foreach (var resource in xmlDataResources)
            {
                var name = resource.GetAttributeValue<string>("name");
                try
                {
                    var xmlConfigData = executionContext.XmlConfigurationResources.Get(name, false);
                    config.AddConfiguration(xmlConfigData);
                }
                catch (Exception ex)
                {
                    executionContext.Trace(Core.eSeverityLevel.Error, "Error processing entity file {0} - {1}", name, ex.Message);
                }
            }

            if(!disableCache && executionContext.Cache != null && settings.CacheTimeout != null)
            {
                executionContext.Cache.Add<IServiceConfiguration>(CACHE_KEY, config, settings.CacheTimeout.Value);
            }

            return config;
           
        }

    }
}
