namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core.Net;
    using CCLLC.Core.Serialization;

    public class GetSecretVersionsRequest : AzureRestRequest<SecretList>
    {        
        public GetSecretVersionsRequest(IJSONContractSerializer serializer, AuthToken token, string vaultName, string secretName) 
            : base(serializer, token, new APIEndpoint(string.Format("https://{0}.vault.azure.net/secrets/{1}/versions?api-version=7.0", vaultName, secretName)))
        {            
        }
    }
}
