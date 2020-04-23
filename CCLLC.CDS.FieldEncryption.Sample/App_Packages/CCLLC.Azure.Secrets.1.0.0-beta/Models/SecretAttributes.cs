using System.Runtime.Serialization;

namespace CCLLC.Azure.Secrets
{
    [DataContract]
    public class SecretAttributes
    {
        [DataMember]
        public bool enabled { get; set; }
        [DataMember]
        public int created { get; set; }
        [DataMember]
        public int updated { get; set; }
    }
}
