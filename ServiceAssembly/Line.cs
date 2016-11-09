using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class Line
    {
        [DataMember]
        public Client Sender { get; set; }

        [DataMember]
        public double X1 { get; set; }
        [DataMember]
        public double Y1 { get; set; }
        [DataMember]
        public double X2 { get; set; }
        [DataMember]
        public double Y2 { get; set; }
        [DataMember]
        public LineSettings Settings { get; set; }


    }
}