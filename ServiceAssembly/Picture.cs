using System.Drawing;
using System.Drawing.Configuration;
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

    }
}