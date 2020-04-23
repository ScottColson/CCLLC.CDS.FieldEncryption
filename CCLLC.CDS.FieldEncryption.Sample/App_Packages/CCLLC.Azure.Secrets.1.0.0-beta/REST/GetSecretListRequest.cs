using CCLLC.Core.Net;
using CCLLC.Core.Serialization;

namespace CCLLC.Azure.Secrets
{
    public class GetSecretListRequest : AzureRestRequest<SecretList>
    {
        public GetSecretListRequest(IJSONContractSerializer serializer, AuthToken token, string vaultName)
           : base(serializer, token, new APIEndpoint(string.Format("https://{0}.vault.azure.net/secrets?api-version=7.0", vaultName)))
        {
        }
    }
}
