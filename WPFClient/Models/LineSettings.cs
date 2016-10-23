using System;

namespace WPFClient.Models
{
    public class LineSettings:ICloneable
    {
        public string Color { get; set; }
        public int Thickness { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}