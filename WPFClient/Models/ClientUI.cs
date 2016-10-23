using System;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight;
using ServiceAssembly;

namespace WPFClient.Models
{
    public class ClientUI:ViewModelBase
    {

        private string _name = string.Empty;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                _pictureUi.Letter = Char.ToUpper(value[0]);
                RaisePropertyChanged(nameof(Name));
            }
        }


        private PictureUI _pictureUi=new PictureUI();
        public  PictureUI PictureUi
        {
            get
            {
                return _pictureUi;
            }

            set
            {
                if (_pictureUi == value)
                {
                    return;
                }

                _pictureUi = value;
                RaisePropertyChanged(nameof(PictureUi));
            }
        }

        public ClientUI()
        {
        }

        public ClientUI(string name, string bgcolor)
        {
            Name = name;
            PictureUi=new PictureUI(bgcolor,Char.ToUpper(name[0]));
        }



    }
}