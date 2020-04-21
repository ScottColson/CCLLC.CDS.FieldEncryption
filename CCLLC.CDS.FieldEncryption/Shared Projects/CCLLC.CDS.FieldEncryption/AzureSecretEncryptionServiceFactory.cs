using System;

namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;
    using CCLLC.Azure.Secrets;

    public class AzureSecretEncryptionServiceFactory : IEncryptionServiceFactory
    {
        private const string CACHE_KEY = "CCLLC.CDS.FieldEncryption.EncryptionServiceFactory";
        
        private readonly ISecretProviderFactory SecretProviderFactory;
        private readonly IEncryptedFieldSettingsFactory SettingsFactory;

        public AzureSecretEncryptionServiceFactory(IEncryptedFieldSettingsFactory settingsFactory, ISecretProviderFactory secretProviderFactory)
        {
            SettingsFactory = settingsFactory;
            SecretProviderFactory = secretProviderFactory;
        }

        public IEncryptionService Create(IProcessExecutionContext executionContext, bool disableCache = false)
        {
            bool useCache = !disableCache && executionContext.Cache != null;

            if (useCache && executionContext.Cache.Exists(CACHE_KEY))
            {
                return executionContext.Cache.Get<IEncryptionService>(CACHE_KEY);
            }

            var settings = SettingsFactory.Create(executionContext);

            var secretProvider = SecretProviderFactory.Create(executionContext, disableCache);

            var encryptionKey = secretProvider.GetValue<string>(settings.EncryptionKeyName) 
                ?? throw new Exception("Missing Secret for encryption key.");

            var encryptor = new DefaultEncryptor(encryptionKey);

            var cacheTimeout = settings.CacheTimeout;

            if(useCache && cacheTimeout.HasValue)
            {
                executionContext.Cache.Add<IEncryptionService>(CACHE_KEY, encryptor, cacheTimeout.Value);
            }

            return encryptor;
        }
    }


    
}
