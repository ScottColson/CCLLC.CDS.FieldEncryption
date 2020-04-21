using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Defines a set of methods for that can be consumed by plugin event handlers to implement field level
    /// data encryption in CDS.
    /// </summary>
    public interface IEncryptedFieldService : IDisposable
    {  
        /// <summary>
        /// Evaluates the input query expression and modifies it to use criteria for searching encrypted data if the
        /// existing query has a search criteria that begins with the <see cref="IServiceConfiguration.EncryptedSearchTrigger"/>
        /// tag.
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        void GenerateEncryptedFieldQuery(ref QueryExpression queryExpression);

        /// <summary>
        /// Modifies configured field values on the input entity by removing any specified formatting characters and 
        /// then encrypting the field data prior to saving it to the CDS data store. Must be executed in the plugin
        /// PreOp pipeline stage.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        void EncryptFields(ref Entity target);       

        /// <summary>
        /// Evaluates the inbound column set to determine if the record being retrieved will return any encrypted 
        /// fields. If so, then it evaluates the field configuration and authorizations for the user to generate 
        /// unmasking instructions that are saved to the plugin <see cref="IExecutionContext.SharedVariables"/>
        /// collection to be used in PostOp by <see cref="DecryptFields(ref Entity)"/> or 
        /// <see cref="DecryptFields(ref EntityCollection)"/> methods. The column set is also modified to remove
        /// any trigger attribute fields.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnSet"></param>
        void PrepareForDecryption(ref ColumnSet columnSet);

        /// <summary>
        /// Evaluates any field masking instructions that were saved to the <see cref="IExecutionContext.SharedVariables"/>
        /// collection by <see cref="PrepareForDecryption(ref ColumnSet)"/> method, and modifies the target entity by
        /// decrypting and masking fields as appropriate.
        /// </summary>
        /// <param name="target"></param>
        void DecryptFields(ref Entity target);

        /// <summary>
        /// Evaluates any field masking instructions that were saved to the <see cref="IExecutionContext.SharedVariables"/>
        /// collection by <see cref="PrepareForDecryption(ref ColumnSet)"/> method, and modifies each record in the target
        /// collection by decrypting and masking fields as appropriate.
        /// </summary>
        /// <param name="targetCollection"></param>
        void DecryptFields(ref EntityCollection targetCollection);
    }
}
