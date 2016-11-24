using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class LineSettings
    {
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int Thickness { get; set; }
    }
}