
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WPFClient.Models;
using WPFClient.SVC;

namespace WPFClient.ViewModels
{
    public class MainPageViewModel:ViewModelBase,IChatCallback
    {
        public SVC.Client LocalClient;
        private delegate void FaultedInvoker();
        private ObservableCollection<SVC.Message> _messagesCollection = new ObservableCollection<SVC.Message>();
        public ObservableCollection<SVC.Message> Messages
        {
            get
            {
                return _messagesCollection;
            }

            set
            {
                if (_messagesCollection == value)
                {
                    return;
                }

                _messagesCollection = value;
                RaisePropertyChanged(nameof(Messages));
            }
        }

        private RelayCommand _loadCompleteCommand;
        public RelayCommand LoadComplete
        {
            get
            {
                return _loadCompleteCommand
                    ?? (_loadCompleteCommand = new RelayCommand(
                    () =>
                    {
                        ViewModelLocator.Proxy.InnerDuplexChannel.Faulted += InnerDuplexChannel_Faulted;
                        ViewModelLocator.Proxy.InnerDuplexChannel.Closed += InnerDuplexChannel_Closed;
                    }));
            }
        }

        private void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        private void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        private string _textField = string.Empty;
        public string TextField
        {
            get
            {
                return _textField;
            }

            set
            {
                if (_textField == value)
                {
                    return;
                }

                _textField = value;
                RaisePropertyChanged(nameof(TextField));
            }
        }

        private Point CurrentPoint { get; set; }

        private ObservableCollection<LineUI> _linesCollection=new ObservableCollection<LineUI>();
        public ObservableCollection<LineUI> LinesCollection
        {
            get
            {
                return _linesCollection;
            }

            set
            {
                if (_linesCollection == value)
                {
                    return;
                }

                _linesCollection = value;
                RaisePropertyChanged(nameof(LinesCollection));
            }
        }


        private LineSettings _lineSettings = new LineSettings() {Color = "black",Thickness = 2};   
        public LineSettings LineSettings
        {
            get
            {
                return _lineSettings;
            }

            set
            {
                if (_lineSettings == value)
                {
                    return;
                }

                _lineSettings = value;
                RaisePropertyChanged(nameof(LineSettings));
            }
        }

        private RelayCommand _sendMessageCommand;
        public RelayCommand SendMessage
        {
            get
            {
                return _sendMessageCommand
                    ?? (_sendMessageCommand = new RelayCommand(
                    () =>
                    {
                        if (string.IsNullOrWhiteSpace(TextField) || ViewModelLocator.Proxy == null) return;

                        if (ViewModelLocator.Proxy.State == CommunicationState.Faulted)
                            HandleProxy();
                        else
                        {
                            
                            SVC.Message msg = new SVC.Message()
                            {
                                Content = TextField,
                                Sender = LocalClient,
                                Time = DateTime.Now.ToLongTimeString()
                            };
                            ViewModelLocator.Proxy.SayAsync(msg);
                            TextField = string.Empty;
                        }
                    }));
            }
        }

        private RelayCommand<MouseArgs> _drawmMouseDownCommand;
        public RelayCommand<MouseArgs> DrawMouseDown
        {
            get
            {
                return _drawmMouseDownCommand
                    ?? (_drawmMouseDownCommand = new RelayCommand<MouseArgs>(
                    (key) =>
                    {
                        if (key.ButtonState == MouseButtonState.Pressed)
                            CurrentPoint = key.Point;
                    }));
            }
        }

        private RelayCommand<MouseArgs> _drawMouseMoveCommand;
        public RelayCommand<MouseArgs> DrawMouseMove
        {
            get
            {
                return  _drawMouseMoveCommand
                    ?? ( _drawMouseMoveCommand = new RelayCommand<MouseArgs>(
                    (key) =>
                    {
                        if (key.ButtonState == MouseButtonState.Pressed)
                        {
                            LineUI line=new LineUI();
                            line.Settings = LineSettings.Clone() as LineSettings;
                            line.X1 = CurrentPoint.X;
                            line.Y1 = CurrentPoint.Y;
                            line.X2 = key.Point.X;
                            line.Y2 = key.Point.Y;

                            CurrentPoint = key.Point;
                            LinesCollection.Add(line);

                        }
                    }));
            }
        }

        private RelayCommand<MouseButtonEventArgs> _drawMouseUpCommand;
        public RelayCommand<MouseButtonEventArgs> DrawMouseUp
        {
            get
            {
                return _drawMouseUpCommand
                    ?? (_drawMouseUpCommand = new RelayCommand<MouseButtonEventArgs>(
                    (key) =>
                    {

                    }));
            }
        }


        private bool _flyOutOpen = false;
        public bool FlyOutIsOpen
        {
            get
            {
                return _flyOutOpen;
            }

            set
            {
                if (_flyOutOpen == value)
                {
                    return;
                }

                _flyOutOpen = value;
                RaisePropertyChanged(nameof(FlyOutIsOpen));
            }
        }

        private RelayCommand _openFlyOutCommand;

        public RelayCommand OpenFlyOut
        {
            get
            {
                return _openFlyOutCommand
                    ?? (_openFlyOutCommand = new RelayCommand(
                    () =>
                    {
                        FlyOutIsOpen = true;
                    }));
            }
        }

        private RelayCommand _exitChatCommand;
        public RelayCommand ExitChat
        {
            get
            {
                return _exitChatCommand
                    ?? (_exitChatCommand = new RelayCommand(
                    () =>
                    {
                        Messages.Clear();
                        Clients.Clear();
                        TextField = string.Empty;
                        LinesCollection.Clear();
                        if (ViewModelLocator.Proxy != null)
                        {
                            if (ViewModelLocator.Proxy.State == CommunicationState.Faulted)
                            {
                                HandleProxy();
                            }
                            else
                            {
                                ViewModelLocator.Proxy.DisconnectAsync(LocalClient);
                            }
                        }
                    }));
            }
        }

        #region IChatCallbackRegion
        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
        public ObservableCollection<Client> Clients
        {
            get
            {
                return _clients;
            }

            set
            {
                if (_clients == value)
                {
                    return;
                }

                _clients = value;
                RaisePropertyChanged(nameof(Clients));
            }
        }

        private void HandleProxy()
        {
            if (ViewModelLocator.Proxy != null)
            {
                switch (ViewModelLocator.Proxy.State)
                {
                    case CommunicationState.Closed:
                        ViewModelLocator.Proxy = null;
                        ViewModelLocator.NavigationService.NavigateTo("LoginPage");
                        break;
                    case CommunicationState.Faulted:
                        ViewModelLocator.Proxy.Abort();
                        ViewModelLocator.Proxy = null;
                        ViewModelLocator.NavigationService.NavigateTo("LoginPage");
                        break;
                    default:
                        break;
                }
            }

        }

        public void RefreshClients(List<Client> clients)
        {
            Clients=new ObservableCollection<Client>(clients);
        }

        public void Receive(Message msg)
        {
            
            Messages.Add(msg);
        }

        public void ReceiveWhisper(Message msg, Client receiver)
        {
            throw new NotImplementedException();
        }

        public void IsWritingCallback(Client client)
        {
            throw new NotImplementedException();
        }

        public void ReceiverFile(FileMessage msg, Client receiver)
        {
            throw new NotImplementedException();
        }


        public void UserJoin(Client client)
        {
            Messages.Add(new SVC.Message() {Content = "User joined",Sender = client,Time=DateTime.Now.ToLongTimeString()} );
        }


        public void UserLeave(Client client)
        {
            Messages.Add(new SVC.Message() { Content = "User lived", Sender = client, Time = DateTime.Now.ToLongTimeString() });
        }

        #endregion

        #region NotImplemented
        public IAsyncResult BeginRefreshClients(List<Client> clients, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndRefreshClients(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceive(Message msg, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceive(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceiveWhisper(Message msg, Client receiver, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceiveWhisper(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginIsWritingCallback(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndIsWritingCallback(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceiverFile(FileMessage msg, Client receiver, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceiverFile(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginUserJoin(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserJoin(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginUserLeave(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserLeave(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}