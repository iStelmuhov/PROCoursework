
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WPFClient.Models;
using WPFClient.SVC;

namespace WPFClient.ViewModels
{
    public class MainPageViewModel:ViewModelBase,IChatCallback
    {
        private SVC.Client LocalClient;
        
        public MainPageViewModel()
        {
            LocalClient = ViewModelLocator.NavigationService.Parameter as SVC.Client;
            ViewModelLocator.MainPage = this;
        }

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
                        if(string.IsNullOrWhiteSpace(TextField)) return;
                        Messages.Add(new Message() {Content = TextField,Sender = LocalClient});
                        TextField = string.Empty;
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
        public bool FlyIsOutOpen
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
                RaisePropertyChanged(nameof(FlyIsOutOpen));
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
                        FlyIsOutOpen = true;
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
                        ViewModelLocator.NavigationService.NavigateTo("LoginPage");
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
            Messages.Add(new SVC.Message() );
        }


        public void UserLeave(Client client)
        {
            throw new NotImplementedException();
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