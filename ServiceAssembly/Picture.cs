using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class Picture
    {
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public char Letter { get; set; }

        public Picture()
        {
            Color = "blue";
        }

        public Picture(string color, char letter)
        {
            Color = color;
            Letter = letter;
        }

        protected bool Equals(Picture other)
        {
            return string.Equals(Color, other.Color) && Letter == other.Letter;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Picture) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Color != null ? Color.GetHashCode() : 0)*397) ^ Letter.GetHashCode();
            }
        }
    }
}