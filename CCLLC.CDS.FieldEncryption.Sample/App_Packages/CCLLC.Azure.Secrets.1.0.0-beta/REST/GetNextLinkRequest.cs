
using System;
using System.Collections.Generic;
using System.Text;

namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core.Net;
    using CCLLC.Core.Serialization;

    public class GetNextLinkRequest : AzureRestRequest<SecretList>
    {
        public GetNextLinkRequest(IJSONContractSerializer serializer, AuthToken token, string nextLink) 
            : base(serializer, token, new APIEndpoint(nextLink))
        {
        }
    }
}
