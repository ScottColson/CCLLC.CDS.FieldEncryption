using System;
using System.Collections.Generic;
using System.Linq;

namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core;
    using CCLLC.Core.Net;
    using CCLLC.Core.Serialization;

    public class AzureSecretProviderFactory : ISecretProviderFactory
    {
        const string CACHE_KEY = "CCLLC.Azure.SecretProviderFactory";

        private struct SettingKeys
        {
            public static readonly string TenantId = "CCLLC.Azure.Secrets.TenantId";
            public static readonly string ClientId = "CCLLC.Azure.Secrets.ClientId";
            public static readonly string ClientSecret = "CCLLC.Azure.Secrets.ClientSecret";
            public static readonly string VaultName = "CCLLC.Azure.Secrets.VaultName";
            public static readonly string CacheTimeout = "CCLLC.Azure.Secrets.CacheTimeout";
        }

        private readonly IWebRequestFactory WebRequestFactory;
        private readonly IJSONContractSerializer Serializer;

        public AzureSecretProviderFactory(IWebRequestFactory webRequestFactory, IJSONContractSerializer serializer)
        {
            WebRequestFactory = webRequestFactory ?? throw new ArgumentNullException("webRequestFactory");
            Serializer = serializer ?? throw new ArgumentNullException("serializer");
        }

        public ISecretProvider Create(IProcessExecutionContext executionContext, bool disableCache = false)
        {
            if(!disableCache && executionContext.Cache != null)
            {
                if (executionContext.Cache.Exists(CACHE_KEY))
                {
                    return executionContext.Cache.Get<ISecretProvider>(CACHE_KEY);
                }
            }

            var tenantId = executionContext.Settings.GetValue<string>(SettingKeys.TenantId) ?? throw new Exception("Missing TenantId setting.");
            var clientId = executionContext.Settings.GetValue<string>(SettingKeys.ClientId) ?? throw new Exception("Missing ClientId setting.");
            var clientSecret = executionContext.Settings.GetValue<string>(SettingKeys.ClientSecret) ?? throw new Exception("Missing ClientSecret setting.");
            var vaultName = executionContext.Settings.GetValue<string>(SettingKeys.VaultName) ?? throw new Exception("Missing VaultName setting.");
            var cacheTimeout = executionContext.Settings.GetValue<TimeSpan?>(SettingKeys.CacheTimeout, TimeSpan.FromMinutes(15));

            var secretIds = GetSecretIds(tenantId, clientId, clientSecret, vaultName);

            var secretProvider = new AzureSecretProvider(WebRequestFactory, Serializer, secretIds, tenantId, clientId, clientSecret, vaultName);
        
            if(!disableCache && executionContext.Cache != null && cacheTimeout.HasValue)
            {
                executionContext.Cache.Add<ISecretProvider>(CACHE_KEY, secretProvider, cacheTimeout.Value);
            }

            return secretProvider;        
        }

        /// <summary>
        /// Generates a dictionary loaded with the ids of all enabled secrets in the key vault indexed by the secret name.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="vaultName"></param>
        /// <returns></returns>
        private IReadOnlyDictionary<string,string> GetSecretIds(string tenantId, string clientId, string clientSecret, string vaultName)
        {
            var dictionary = new Dictionary<string, string>();

            var token = GetToken(tenantId, clientId, clientSecret);

            string nextLink = null;

            var request = new GetSecretListRequest(Serializer, token, vaultName);

            using (var webRequest = WebRequestFactory.CreateWebRequest(request.ApiEndpoint)) 
            {
                var response = request.Execute(webRequest);

                nextLink = response.nextLink;

                foreach(var secretItem in response.value.Where(s => s.attributes.enabled))
                {
                    dictionary.Add(ParseKeyFromId(secretItem.id), secretItem.id);
                }
            }

            while (!string.IsNullOrEmpty(nextLink))
            {    
                var nextLinkRequest = new GetNextLinkRequest(Serializer, token, nextLink);

                using (var webRequest = WebRequestFactory.CreateWebRequest(nextLinkRequest.ApiEndpoint))
                {
                    var response = nextLinkRequest.Execute(webRequest);

                    nextLink = response.nextLink;

                    foreach (var secretItem in response.value.Where(s => s.attributes.enabled))
                    {
                        dictionary.Add(
                            ParseKeyFromId(secretItem.id), 
                            secretItem.id);
                    }
                }
            }

            return dictionary;
        }

        private string ParseKeyFromId(string secretId)
        {            
            return secretId?.Split('/').Last();
        }

        private AuthToken GetToken(string tenantId, string clientId, string clientSecret)
        {
            var request = new GetAuthTokenRequest(this.Serializer, tenantId, clientId, clientSecret);

            using (var webRequest = WebRequestFactory.CreateWebRequest(request.ApiEndpoint))
            {
                return request.Execute(webRequest);
            }            
        }        
    }
}
