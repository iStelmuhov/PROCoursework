using System;
using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class Client
    {
        private string _name;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        [DataMember]
        public Picture Pic { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public DateTime Time { get; set; }
        public Client()
        {
            Pic=new Picture("blue",' ');
            Time = DateTime.Now;
        }

        public Client(string name, Picture pic)
        {
            Pic = pic;
            Name = name;
            Time = DateTime.Now;
        }

        public Client(string name, string bgcolor)
        {
            Name = name;
            Pic = new Picture(bgcolor, Char.ToUpper(name[0]));
            Time = DateTime.Now;
        }

        protected bool Equals(Client other)
        {
            return string.Equals(_name, other._name) && Equals(Pic, other.Pic);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Client) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name?.GetHashCode() ?? 0)*397) ^ (Pic?.GetHashCode() ?? 0);
            }
        }
    }
}