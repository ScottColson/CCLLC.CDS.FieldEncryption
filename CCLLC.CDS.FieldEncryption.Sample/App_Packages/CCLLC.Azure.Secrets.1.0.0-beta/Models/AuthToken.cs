using System.Runtime.Serialization;
using CCLLC.Core.Serialization;
using CCLLC.Core.RESTClient;

namespace CCLLC.Azure.Secrets
{
    [DataContract]
    public class AuthToken : ISerializedRESTResponse
    {
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string ext_expires_in { get; set; }
        [DataMember]
        public string expires_on { get; set; }
        [DataMember]
        public string not_before { get; set; }
        [DataMember]
        public string resource { get; set; }
        [DataMember]
        public string access_token { get; set; }

        public string ToString(IDataSerializer serializer)
        {
            return serializer.Serialize<AuthToken>(this);
        }
    }
}
