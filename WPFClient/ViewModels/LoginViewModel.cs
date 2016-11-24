using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.ServiceLocation;
using WPFClient.Models;
using WPFClient.SVC;
using WPFClient.Views;
using Application = System.Windows.Application;

namespace WPFClient.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        public LoginViewModel()
        {
            _localClient = new SVC.Client() { Name = string.Empty, Pic = new Picture() { Color = PictureUI.GetRandomColor(), Letter = ' ' } };
        }

        public delegate Task TaskInvoker();

        private RelayCommand _loadCompletedCommand;
        public RelayCommand LoadedCommand
        {
            get
            {
                return _loadCompletedCommand
                    ?? (_loadCompletedCommand = new RelayCommand(
                    () =>
                    {
                        ThreadPool.QueueUserWorkItem((o) =>
                        {
                            Thread.Sleep(500);
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new TaskInvoker(HideVisibleDialogs));

                        });

                    }));
            }
        }

        private SVC.GameClient _proxy = ViewModelLocator.Proxy;

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

        private string _selectedIp = "localhost:7997";
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
                if (LocalClient.Name == value)
                    return;

                LocalClient.Name = value;
                LocalClient.Pic.Letter = char.ToUpper(value[0]);
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(LocalClient.Pic.Letter));
            }
        }

        #region RegExp
        /// <summary>
        /// Determines whether the username meets conditions.
        /// Username conditions:
        /// Must be 1 to 12 character in length
        /// Must start with letter a-zA-Z
        /// May contain letters, numbers or '.','-' or '_'
        /// Must not end in '.','-','._' or '-_' 
        /// </summary>
        private const string NameRegExp = @"^(?=[a-zA-Z])[-\w.]{0,11}([a-zA-Z\d]|(?<![-.])_)$";

        private const string IpPortRegExp = @"((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?):(\d){4}";
        private const string ValidationPetternLocalhost = @"localhost:[0-9]{4}";
        #endregion



        private RelayCommand _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand
                    ?? (_loginCommand = new RelayCommand(async () =>
                       {
                           if (!Regex.Match(Name, NameRegExp).Success)
                           {
                               Message("Wrong name format");
                               return;
                           }

                           string ip, port;
                           if (Regex.Match(SelectedIp, IpPortRegExp).Success | Regex.Match(SelectedIp,ValidationPetternLocalhost).Success)
                           {
                               string[] split = SelectedIp.Split(':');
                               ip = split[0];
                               port = split[1];
                           }
                           else
                           {
                               Message("Wrong IP adress");
                               return;
                           }

                           _proxy = null;
                           try
                           {
                               await ShowProgressIndicator();
                               var mainPageModel = ServiceLocator.Current.GetInstance<MainPageViewModel>();
                               mainPageModel.LocalClient = _localClient;
                               InstanceContext context = new InstanceContext(mainPageModel);
                               _proxy = new SVC.GameClient(context);

                               string servicePath = _proxy.Endpoint.ListenUri.AbsolutePath;

                               _proxy.Endpoint.Address = new EndpointAddress($"net.tcp://{ip}:{port}{servicePath}");

                               _proxy.Open();

                               _proxy.ConnectAsync(LocalClient);
                               _proxy.ConnectCompleted += _proxy_ConnectCompleted;
                               ViewModelLocator.Proxy = _proxy;
                           }
                           catch (Exception ex)
                           {
                               await HideVisibleDialogs();
                               Message(ex.Message);
                           }
                       }));
            }
        }

        private async void _proxy_ConnectCompleted(object sender, SVC.ConnectCompletedEventArgs e)
        {
            await HideVisibleDialogs();
            if (e.Error != null)
            {
                Message(e.Error.Message);
            }
            else if (e.Result)
            {
                await HandleProxy();
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
            await DialogCoordinator.Instance.ShowMessageAsync(this, "Exception", msg, MessageDialogStyle.Affirmative, materialSettings);
        }

        public static Task HideVisibleDialogs()
        {
            return Task.Run(async () =>
            {

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var parent = Application.Current.MainWindow as MetroWindow;
                    BaseMetroDialog dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();

                    while (dialogBeingShow != null)
                    {
                        await parent.HideMetroDialogAsync(dialogBeingShow);
                        dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();
                    }
                });
            });
        }
        private async Task HandleProxy()
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
                    await HideVisibleDialogs();
                    ViewModelLocator.NavigationService.NavigateTo("MainPage", LocalClient);
                    break;
                default:
                    break;

            }
        }

        private async Task ShowProgressIndicator()
        {
            var circularProgressBarDialog = new CustomDialog() { Content = new CircularProgressBar() };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, circularProgressBarDialog);
            
        }

    }
}