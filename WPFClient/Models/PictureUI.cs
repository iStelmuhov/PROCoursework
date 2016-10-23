using System;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using MaterialDesignColors;

namespace WPFClient.Models
{
    public class PictureUI:ViewModelBase
    {

        private string _color;
        public string Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                RaisePropertyChanged(nameof(Color));
            }
        }


        private char _letter;
        public char Letter
        {
            get
            {
                return _letter;
            }

            set
            {
                if (_letter == value)
                {
                    return;
                }

                _letter = value;
                RaisePropertyChanged(nameof(Letter));
            }
        }

        public PictureUI()
        {
            Color = GetRandomColor();
        }

        public PictureUI(string color, char letter)
        {
            _color = color;
            _letter = letter;
        }
    
        public SVC.Picture ToPicture()
        {
            return new SVC.Picture() { Color = Color,Letter = _letter};
        }
        public  static string GetRandomColor()
        {
            var sw = new SwatchesProvider().Swatches.ToList();
            var hues = sw.ElementAt(new Random().Next(0, sw.Count)).PrimaryHues.ToList();
            return hues[new Random().Next(0, hues.Count)].Color.ToString();
        }
    }
}