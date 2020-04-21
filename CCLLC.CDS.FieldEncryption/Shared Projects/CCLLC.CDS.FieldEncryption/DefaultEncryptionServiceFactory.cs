namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;

    public class DefaultEncryptionServiceFactory : IEncryptionServiceFactory
    {
        private const string CACHE_KEY = "CCLLC.CDS.FieldEncryption.EncryptionServiceFactory";
        
        private readonly IEncryptedFieldSettingsFactory SettingsFactory;

        public DefaultEncryptionServiceFactory(IEncryptedFieldSettingsFactory settingsFactory)
        {
            SettingsFactory = settingsFactory;            
        }

        public IEncryptionService Create(IProcessExecutionContext executionContext, bool disableCache = false)
        {
            bool useCache = !disableCache && executionContext.Cache != null;

            if (useCache && executionContext.Cache.Exists(CACHE_KEY))
            {
                return executionContext.Cache.Get<IEncryptionService>(CACHE_KEY);
            }

            var settings = SettingsFactory.Create(executionContext);

            var encryptor = new DefaultEncryptor();

            var cacheTimeout = settings.CacheTimeout;

            if(useCache && cacheTimeout.HasValue)
            {
                executionContext.Cache.Add<IEncryptionService>(CACHE_KEY, encryptor, cacheTimeout.Value);
            }

            return encryptor;
        }
    }   
}
