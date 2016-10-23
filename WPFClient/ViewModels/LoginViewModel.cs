﻿using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.ServiceLocation;
using WPFClient.Models;
using WPFClient.SVC;

namespace WPFClient.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        public LoginViewModel()
        {
            _localClient=new SVC.Client() {Name = string.Empty,Pic = new Picture() { Color = PictureUI.GetRandomColor(),Letter = ' '}};
        }

        private SVC.ChatClient _proxy = ViewModelLocator.Proxy;

        private SVC.Client _localClient;
        public SVC.Client LocalClient
        {
            get
            {
                return _localClient;
            }

            set
            {
                if (_localClient == value)
                {
                    return;
                }

                _localClient = value;
                RaisePropertyChanged(nameof(LocalClient));
            }
        }


        private bool _osHostDialogOpen;
        public bool IsHostDialogOpen
        {
            get
            {
                return _osHostDialogOpen;
            }

            set
            {
                if (_osHostDialogOpen == value)
                {
                    return;
                }

                _osHostDialogOpen = value;
                RaisePropertyChanged(nameof(IsHostDialogOpen));
            }
        }


        private object _dialogHostContent;
        public object DialogHostContent
        {
            get
            {
                return _dialogHostContent;
            }

            set
            {
                if (_dialogHostContent == value)
                {
                    return;
                }

                _dialogHostContent = value;
                RaisePropertyChanged(nameof(DialogHostContent));
            }
        }


        private ObservableCollection<string> _serversIPsCollection = new ObservableCollection<string>() { "localhost" };
        public ObservableCollection<string> ServersIpCollection
        {
            get
            {
                return _serversIPsCollection;
            }

            set
            {
                if (_serversIPsCollection == value)
                {
                    return;
                }

                _serversIPsCollection = value;
                RaisePropertyChanged(nameof(ServersIpCollection));
            }
        }

        private string _selectedIp = string.Empty;
        public string SelectedIp
        {
            get
            {
                return _selectedIp;
            }

            set
            {
                if (_selectedIp == value)
                {
                    return;
                }

                _selectedIp = value;
                RaisePropertyChanged(nameof(SelectedIp));
            }
        }


        private string _newServerIp = string.Empty;
        public string NewSelectedServerIp
        {
            set
            {
                if (SelectedIp != null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    _serversIPsCollection.Add(value);
                    SelectedIp = value;
                }
            }
        }

        private RelayCommand _changecolorCommand;
        public RelayCommand ChangeColorCommand
        {
            get
            {
                return _changecolorCommand
                    ?? (_changecolorCommand = new RelayCommand(
                    () =>
                    {
                            LocalClient.Pic.Color = PictureUI.GetRandomColor();
                    }));
            }
        }

        public string Name
        {
            get { return LocalClient.Name; }
            set
            {
                if(LocalClient.Name==value)
                    return;

                LocalClient.Name = value;
                LocalClient.Pic.Letter = char.ToUpper(value[0]);
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(LocalClient.Pic.Letter));
            }
        }

        private RelayCommand _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand
                    ?? (_loginCommand = new RelayCommand(() =>
                       {
                           _proxy = null;
                      try
                      {
                          

                          InstanceContext context = new InstanceContext(ServiceLocator.Current.GetInstance<MainPageViewModel>());
                          _proxy = new SVC.ChatClient(context);

                          string servicePath = _proxy.Endpoint.ListenUri.AbsolutePath;
                          string serviceListenPort = _proxy.Endpoint.Address.Uri.Port.ToString();

                          _proxy.Endpoint.Address = new EndpointAddress($"net.tcp://{SelectedIp}:{serviceListenPort}{servicePath}");

                          _proxy.Open();

                          _proxy.ConnectAsync(LocalClient);
                          _proxy.ConnectCompleted += _proxy_ConnectCompleted;
                      }
                      catch (Exception ex)
                      {
                          Message(ex.Message);
                      }


                  }));
            }
        }

        private void _proxy_ConnectCompleted(object sender, SVC.ConnectCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Message(e.Error.Message);
            }
            else if(e.Result)
            {
                HandleProxy();
            }
            else if (!e.Result)
            {
                Message("Name found");
            }
        }

        private async void Message(string msg)
        {
            var materialSettings = new MetroDialogSettings
            {
                CustomResourceDictionary =
                                         new ResourceDictionary()
                                         {
                                             Source =
                                                 new Uri(
                                                     "pack://application:,,,/MaterialDesignThemes.MahApps;component/Themes/MaterialDesignTheme.MahApps.Dialogs.xaml")
                                         },
                SuppressDefaultResources = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };
            IsHostDialogOpen = false;
            await DialogCoordinator.Instance.ShowMessageAsync(this, "Exception", msg, MessageDialogStyle.Affirmative, materialSettings);

        }


        private void HandleProxy()
        {
            switch (_proxy.State)
            {
                case CommunicationState.Closed:
                    _proxy = null;
                    Message("Dissconnected");
                    break;
                case CommunicationState.Faulted:
                    _proxy.Abort();
                    _proxy = null;
                    Message("Dissconnected");
                    break;
                case CommunicationState.Opened:
                    ViewModelLocator.NavigationService.NavigateTo("MainPage", LocalClient);
                    break;
                default:
                    break;

            }
        }

    }
}