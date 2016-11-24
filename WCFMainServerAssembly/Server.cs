using System.Runtime.Serialization;

namespace WCFMainServerAssembly
{
    [DataContract]
    public class Server
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string IpAdress { get; set; }
        [DataMember]
        public string PortNumber { get; set; }
    }
}