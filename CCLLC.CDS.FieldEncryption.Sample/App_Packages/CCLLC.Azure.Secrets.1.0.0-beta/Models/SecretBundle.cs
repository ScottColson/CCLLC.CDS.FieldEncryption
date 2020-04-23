using System.Runtime.Serialization;
using CCLLC.Core.Serialization;
using CCLLC.Core.RESTClient;

namespace CCLLC.Azure.Secrets
{
    [DataContract]
    public class SecretBundle : ISerializedRESTResponse
    {
        [DataMember]
        public string value { get; set; }
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public SecretAttributes attributes { get; set; }

        public string ToString(IDataSerializer serializer)
        {
            return serializer.Serialize<SecretBundle>(this);
        }
    }
}
