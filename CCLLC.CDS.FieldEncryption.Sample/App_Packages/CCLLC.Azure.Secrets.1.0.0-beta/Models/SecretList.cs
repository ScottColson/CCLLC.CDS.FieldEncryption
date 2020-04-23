using System.Runtime.Serialization;

namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core.Serialization;
    using CCLLC.Core.RESTClient;

    [DataContract]
    public class SecretList : ISerializedRESTResponse
    {        
        [DataMember]
        public SecretItem[] value { get; set; }
        [DataMember]
        public string nextLink { get; set; }

        public string ToString(IDataSerializer serializer)
        {
            return serializer.Serialize<SecretList>(this);
        }
    }
}
