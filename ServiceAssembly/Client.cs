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
        public DateTime Time { get; set; }

        public Client()
        {
            Pic=new Picture("blue",' ');
        }

        public Client(string name, Picture pic, DateTime time)
        {
            Pic = pic;
            Time = time;
            Name = name;
        }

        public Client(string name, string bgcolor)
        {
            Name = name;
            Pic = new Picture(bgcolor, Char.ToUpper(name[0]));
        }
    }
}