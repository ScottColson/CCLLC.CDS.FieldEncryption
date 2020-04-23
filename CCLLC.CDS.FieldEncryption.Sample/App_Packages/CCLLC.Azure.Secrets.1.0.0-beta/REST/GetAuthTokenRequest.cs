using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CCLLC.Core.Net;
using CCLLC.Core.Serialization;
using CCLLC.Core.RESTClient;

namespace CCLLC.Azure.Secrets
{
    public class GetAuthTokenRequest : SerializedRESTRequest<AuthToken>
    {
        private readonly string ClientId;
        private readonly string ClientSecret;       

        public GetAuthTokenRequest(IJSONContractSerializer serializer, string tenantId, string clientId, string clientSecret) 
            : base(serializer, new APIEndpoint("https://login.windows.net/" + tenantId + "/oauth2/token"))
        {                    
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;           
        }

        public override AuthToken Execute(IWebRequest webRequest)
        {
            var data = generateFormData();
            SetRequestHeaders(webRequest);
            var webResponse = webRequest.Post(data, "application/x-www-form-urlencoded");

            return Serializer.Deserialize<AuthToken>(webResponse.Content);
        }

        private byte[] generateFormData()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("resource", "https://vault.azure.net"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var stream = new MemoryStream();
            formContent.CopyToAsync(stream).Wait();
            return stream.ToArray();
        }
    }
}
