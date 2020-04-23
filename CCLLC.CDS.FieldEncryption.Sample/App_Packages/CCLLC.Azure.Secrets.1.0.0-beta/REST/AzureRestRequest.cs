using System;
using System.Text;


namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core.Net;
    using CCLLC.Core.RESTClient;
    using CCLLC.Core.Serialization;

    public abstract class AzureRestRequest<T> : SerializedRESTRequest<T> where T : class, ISerializedRESTResponse
    {
        protected AzureRestRequest(IJSONContractSerializer serializer, AuthToken token, IAPIEndpoint endpoint) 
            : base(serializer, endpoint, token?.access_token)        
        {           
        }

        protected virtual string AuthenticationToken
        {
            get
            {
                return string.Format("Bearer {0}", this.AccessToken);
            }
        }

        public override T Execute(IWebRequest webRequest)
        {
            webRequest.Headers.Add("Authorization", this.AuthenticationToken);
            webRequest.Headers.Add("Cache-Control", "no-cache");

            return InternalExecute(this.Serializer, webRequest);
            
        }

       
        protected virtual T InternalExecute(IDataSerializer serializer, IWebRequest webRequest)
        {
            var webResponse = webRequest.Get();
            return serializer.Deserialize<T>(webResponse.Content);
        }
    }
}
