using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Input;

namespace ServiceAssembly
{
    [DataContract]
    public class Line
    {
        [DataMember]
        public Client Sender { get; set; }

        [DataMember]
        public DrawingAttributes Attributes { get; set; }

        [DataMember]
        public StylusPointCollection Points { get; set; }

        [DataMember]
        public bool IsRecived { get; set; }
        protected bool Equals(Line other)
        {
            return Points.Equals(other.Points);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Line) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Attributes != null ? Attributes.GetHashCode() : 0)*397) ^ (Points != null ? Points.GetHashCode() : 0);
            }
        }
    }
}