using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CCLLC.CDS.FieldEncryption.Sample
{
    using CCLLC.Core;
    using CCLLC.Core.Serialization;
    using CCLLC.CDS.Sdk;
    using CCLLC.Azure.Secrets;

    public class FieldEncryptionPlugin : CDSPlugin
    {
        public FieldEncryptionPlugin(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
            Container.Implement<IEncryptedFieldSettingsFactory>().Using<EncryptedFieldSettingsFactory>().AsSingleInstance();
            Container.Implement<IEncryptedFieldServiceFactory>().Using<EncryptedFieldServiceFactoryNoLogging>().AsSingleInstance();
            Container.Implement<IEncryptionServiceFactory>().Using<AzureSecretEncryptionServiceFactory>().AsSingleInstance();
            Container.Implement<ISecretProviderFactory>().Using<AzureSecretProviderFactory>().AsSingleInstance();
            Container.Implement<IJSONContractSerializer>().Using<DefaultJSONSerializer>().AsSingleInstance();
            Container.Implement<IUserRoleEvaluator>().Using<UserSecurityRoleEvaluator>().AsSingleInstance();


            RegisterCreateHandler<Entity>(ePluginStage.PreOperation, EncryptOnCreateHandler);
            RegisterUpdateHandler<Entity>(ePluginStage.PreOperation, EncryptOnUpdateHandler);

            RegisterRetrieveHandler<Entity>(ePluginStage.PreOperation, PrepareForRetrieveDecryption);
            RegisterRetrieveHandler<Entity>(ePluginStage.PostOperation, PostRetrieveDecryption);

            RegisterQueryHandler<Entity>(ePluginStage.PreOperation, EncryptedSearchHandler);
            RegisterQueryHandler<Entity>(ePluginStage.PreOperation, PrepareForQueryDecryption);
            RegisterQueryHandler<Entity>(ePluginStage.PostOperation, PostQueryDecryption);
        }


        private void EncryptOnCreateHandler(ICDSPluginExecutionContext executionContext, Entity target, EntityReference createdRecordId)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, target.LogicalName))
            {
                encryptionService.EncryptFields(ref target);
            }
        }

        private void EncryptOnUpdateHandler(ICDSPluginExecutionContext executionContext, Entity target)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, target.LogicalName))
            {
                encryptionService.EncryptFields(ref target);
            }
        }


        private void EncryptedSearchHandler(ICDSPluginExecutionContext executionContext, QueryExpression query, EntityCollection entityCollection)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, query.EntityName))
            {
                encryptionService.GenerateEncryptedFieldQuery(ref query);
            }
        }


        private void PrepareForRetrieveDecryption(ICDSPluginExecutionContext executionContext, EntityReference target, ColumnSet columnSet, Entity retrievedRecord)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, target.LogicalName))
            {
                encryptionService.PrepareForDecryption(ref columnSet);
            }
        }

        private void PostRetrieveDecryption(ICDSPluginExecutionContext executionContext, EntityReference target, ColumnSet columnSet, Entity retrievedRecord)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, target.LogicalName))
            {
                encryptionService.DecryptFields(ref retrievedRecord);
            }
        }


        private void PrepareForQueryDecryption(ICDSPluginExecutionContext executionContext, QueryExpression query, EntityCollection entityCollection)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, query.EntityName))
            {
                var columnSet = query.ColumnSet;
                encryptionService.PrepareForDecryption(ref columnSet);
            }
        }

        private void PostQueryDecryption(ICDSPluginExecutionContext executionContext, QueryExpression query, EntityCollection entityCollection)
        {
            var factory = Container.Resolve<IEncryptedFieldServiceFactory>();

            using (var encryptionService = factory.Create(executionContext, entityCollection.EntityName))
            {
                encryptionService.DecryptFields(ref entityCollection);
            }
        }
    }
}
