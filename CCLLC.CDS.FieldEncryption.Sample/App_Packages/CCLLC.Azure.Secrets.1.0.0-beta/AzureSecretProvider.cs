using System;
using System.Collections.Generic;
using System.Linq;

namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core;
    using CCLLC.Core.Net;
    using CCLLC.Core.Serialization;

    public class AzureSecretProvider : SettingsProvider, ISecretProvider
    {
        private readonly Dictionary<string, string> Secrets;
        private readonly IJSONContractSerializer Serializer;
        private readonly IWebRequestFactory WebRequestFactory;

        private readonly string TenantId;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string VaultName;
      
        public AzureSecretProvider(IWebRequestFactory webRequestFactory, IJSONContractSerializer serializer, IReadOnlyDictionary<string,string> secretIds,  string tenantId, string clientId, string clientSecret, string vaultName) 
            : base(secretIds)
        {
            this.WebRequestFactory = webRequestFactory ?? throw new ArgumentNullException("webRequestFactory");
            this.Serializer = serializer ?? throw new ArgumentNullException("serializer");
            this.TenantId = tenantId ?? throw new ArgumentNullException("tenantId");
            this.ClientId = clientId ?? throw new ArgumentNullException("clientId");
            this.ClientSecret = clientSecret ?? throw new ArgumentNullException("clientSecret");
            this.VaultName = vaultName ?? throw new ArgumentNullException("vautlName");
            
            this.Secrets = new Dictionary<string, string>();
        }


        #region Overrides

        /********************************************************
         * 
         * These overrides of SettingsProvider target all dictionary
         * related properties that involve dictionary values such
         * that the results come from the Secrets dictionary in this
         * class rather than the KeyValuePairs dictionary in the base
         * class and handle loading of the current version of a 
         * secret from the Azure Key Vault only when it is first
         * accessed. 
         * 
         * ******************************************************/

        public override string this[string key]
        {
            get
            {
                if (base.ContainsKey(key) && !Secrets.ContainsKey(key))
                {
                    GetSecret(key);
                }

                return Secrets[key];
            }
        }

        public override bool TryGetValue(string key, out string value)
        {
            if (base.ContainsKey(key) && !Secrets.ContainsKey(key))
            {
                GetSecret(key);
            }

            return Secrets.TryGetValue(key, out value);
        }

        public override IEnumerable<string> Values
        {
            get
            {
                GetAllSecrets(base.KeyValuePairs);
                return Secrets.Values;
            }
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            GetAllSecrets(base.KeyValuePairs);
            return Secrets.GetEnumerator();
        }

        #endregion Overrides


        private void GetAllSecrets(IReadOnlyDictionary<string, string> secretIds, AuthToken token = null)
        {
            if (Secrets.Count < secretIds.Count)
            {
                if (token is null)
                    token = GetToken();

                foreach (var key in secretIds.Keys)
                {
                    if (!Secrets.ContainsKey(key))
                    {
                        GetSecret(key, token);
                    }
                }
            }
        }

        private void GetSecret(string key, AuthToken token = null)
        {
            if (token is null)
                token = GetToken();

            var versionRequest = new GetSecretVersionsRequest(Serializer, token, VaultName, key);

            var webRequest = WebRequestFactory.CreateWebRequest(versionRequest.ApiEndpoint);

            var response = versionRequest.Execute(webRequest);

            if (response is null || response.value.Length == 0)
            {
                Secrets.Add(key, null);
                return;
            }

            var curentVersion = response.value
                .Where(v => v.attributes.enabled)
                .OrderByDescending(v => v.attributes.created)
                .FirstOrDefault()?.id;

            if (string.IsNullOrEmpty(curentVersion))
            {
                Secrets.Add(key, null);
                return;
            }

            var secretRequest = new GetSecretRequest(Serializer, token, curentVersion);
            webRequest = WebRequestFactory.CreateWebRequest(secretRequest.ApiEndpoint);
            var value = secretRequest.Execute(webRequest)?.value;

            Secrets.Add(key, value);
        }
                
        private AuthToken GetToken()
        {
            var request = new GetAuthTokenRequest(this.Serializer, TenantId, ClientId, ClientSecret);

            var webRequest = new HttpWebRequestWrapper(request.ApiEndpoint);

            return request.Execute(webRequest);
        }
    }
}
