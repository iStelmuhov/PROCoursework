using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using WPFClient.Behavior;
using WPFClient.Models;
using WPFClient.SVC;
using WPFClient.Views;
using static WPFClient.Behavior.LineSettingClone;
using Application = System.Windows.Application;
using Message = WPFClient.SVC.Message;

namespace WPFClient.ViewModels
{
    public class MainPageViewModel:ViewModelBase,IGameCallback
    {
        public SVC.Client LocalClient;
        private delegate void FaultedInvoker();
        public delegate Task TaskInvoker();
        
        #region Chat
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


        #endregion

        #region Drawing


        private bool _isDrawer = true;
        public bool IsDrawer
        {
            get
            {
                return _isDrawer;
            }

            set
            {
                if (_isDrawer == value)
                {
                    return;
                }

                _isDrawer = value;
                RaisePropertyChanged(nameof(IsDrawer));
            }
        }

        private StrokeCollection _strokesCollection = new StrokeCollection();
        public StrokeCollection Strokes
        {
            get
            {
                return _strokesCollection;
            }

            set
            {
                if (_strokesCollection == value)
                {
                    return;
                }

                _strokesCollection = value;
                RaisePropertyChanged(nameof(Strokes));
            }
        }
        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            foreach (var stroke in e.Added)
            {
                ViewModelLocator.Proxy.SendLineAsync(new Line() { Attributes = DrawAttributes, Points = stroke.StylusPoints.ToList(), Sender = LocalClient });
            }
            RaisePropertyChanged(nameof(Strokes));
        }

        public bool SendBlock = false;

        private DrawingAttributes _drawAttributes = new DrawingAttributes() {Color=Color.FromRgb(0,0,0),FitToCurve = true,Width = 4,Height = 4}; 
        public DrawingAttributes DrawAttributes
        {
            get
            {
                return _drawAttributes;
            }

            set
            {
                if (_drawAttributes == value)
                {
                    return;
                }

                _drawAttributes = value;
                RaisePropertyChanged(nameof(DrawAttributes));
            }
        }

        public double LineThickness
        {
            get { return _drawAttributes.Width; }
            set
            {
                if(Math.Abs(_drawAttributes.Width - value) < 0.1)
                    return;
                _drawAttributes.Width = value;
                _drawAttributes.Height = value;
            }
        }

        private RelayCommand _clearCommand;
        public RelayCommand ClearLines
        {
            get
            {
                return _clearCommand
                    ?? (_clearCommand = new RelayCommand(
                    () =>
                    {
                        ViewModelLocator.Proxy.ClearLinesAsync(LocalClient);
                    }));
            }
        }  
        #endregion

        #region Flyot

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

        #endregion


        #region Events
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

                        Application.Current.Exit += Current_Exit;

                        ThreadPool.QueueUserWorkItem((o) =>
                        {
                            Thread.Sleep(500);
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new TaskInvoker(HideVisibleDialogs));

                        });

                        ViewModelLocator.Proxy.GetStartInformation(LocalClient);

                        Strokes.StrokesChanged += Strokes_StrokesChanged; ;
                    }));
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
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

        #endregion


        private RelayCommand _exitChatCommand;
        public RelayCommand ExitChat
        {
            get
            {
                return _exitChatCommand
                    ?? (_exitChatCommand = new RelayCommand(
                    () =>
                    {     
                                         
                        if (ViewModelLocator.Proxy != null)
                        {
                            if (ViewModelLocator.Proxy.State == CommunicationState.Faulted)
                            {
                                HandleProxy();
                            }
                            else
                            {
                                ShowProgressIndicator();
                                ViewModelLocator.Proxy.DisconnectAsync(LocalClient);
                            }
                        }
                    }));
            }
        }


        private async Task<MessageDialogResult> ShowMessage(string title,string message, MessageDialogStyle dialogStyle)
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
            
            return await DialogCoordinator.Instance.ShowMessageAsync(this,title, message, dialogStyle, materialSettings);
        }      
        public static Task HideVisibleDialogs(MetroWindow parent)
        {
            return Task.Run(async () =>
            {
                await parent.Dispatcher.Invoke(async () =>
                {
                    BaseMetroDialog dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();
                    while (dialogBeingShow != null)
                    {
                        await parent.HideMetroDialogAsync(dialogBeingShow);
                        dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();
                    }
                });
            });
        }

        private static Task HideVisibleDialogs()
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

        private async void HandleProxy()
        {
            if (ViewModelLocator.Proxy != null)
            {
                switch (ViewModelLocator.Proxy.State)
                {
                    case CommunicationState.Closed:
                        ViewModelLocator.Proxy = null;
                        Messages.Clear();
                        Clients.Clear();
                        TextField = string.Empty;
                        Strokes.Clear();
                        Letters.Clear();
                        await HideVisibleDialogs(Application.Current.MainWindow as MetroWindow);
                        
                        ViewModelLocator.NavigationService.NavigateTo("LoginPage");
                        break;
                    case CommunicationState.Faulted:
                        ViewModelLocator.Proxy.Abort();                    
                        break;
                    default:
                        break;
                }
            }

        }

        private ObservableCollection<char> _lettersList = new ObservableCollection<char>();
        public ObservableCollection<char> Letters
        {
            get
            {
                return _lettersList;
            }

            set
            {
                if (_lettersList == value)
                {
                    return;
                }

                _lettersList = value;
                RaisePropertyChanged(nameof(Letters));
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

        public void ReceiveLine(Line line)
        {
            Strokes.StrokesChanged -= Strokes_StrokesChanged;
            Strokes.Add(new Stroke(new StylusPointCollection(line.Points),line.Attributes));
            Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        public void RefreshLines(List<Line> line)
        {
            Strokes.StrokesChanged -= Strokes_StrokesChanged;
            Strokes.Clear();
            foreach (var l in line)
            {
                Strokes.Add(new Stroke(new StylusPointCollection(l.Points), l.Attributes));
            }
            Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        public void UserJoin(Client client)
        {
            Messages.Add(new SVC.Message() {Content = "User joined",Sender = client,Time=DateTime.Now.ToLongTimeString()} );
        }

        public void UserLeave(Client client)
        {
            Messages.Add(new SVC.Message() { Content = "User lived", Sender = client, Time = DateTime.Now.ToLongTimeString() });
        }

        public void IsWritingCallback(Client client)
        {
            throw new NotImplementedException();
        }

        public void ReciveLetter(char letter, int position)
        {
            Letters[position] = letter;
        }

        public void ReciveWordInfo(int len)
        {
            Letters= MasckedCharList(len, ' ');
        }
        
        public async void DrawerRequest()
        {
            var task = ShowMessage("Drawer request", "Are you want be a drawer?",
                MessageDialogStyle.AffirmativeAndNegative);
            if (await Task.WhenAny(task, Task.Delay(4000)) == task)
            {
                var result = await task;
                ViewModelLocator.Proxy.DrawerResponce(LocalClient, result == MessageDialogResult.Affirmative);
            }
            else
            {

                await HideVisibleDialogs(Application.Current.MainWindow as MetroWindow);
                ViewModelLocator.Proxy.DrawerResponce(LocalClient, false);
            }

            
        }

        public void WordChoose(List<string> words )
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
                AffirmativeButtonText = "Ok",
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };


            CustomDialog dialog = new CustomDialog();
            
            var content = new SelectDialog() { Words = { ItemsSource = words,SelectedIndex = 0} };            
            content.OkButton.Click += (sender, args) =>
            {
                DialogCoordinator.Instance.HideMetroDialogAsync(this, dialog, materialSettings);
                ViewModelLocator.Proxy.ReciveGameWordAsync(content.Words.SelectedItem.ToString().ToUpper(), LocalClient);
            };
            dialog.Content = content;


            DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog,
                  materialSettings);
        }

        private void ShowProgressIndicator()
        {
            var circularProgressBarDialog = new CustomDialog() {Content = new CircularProgressBar()};
            DialogCoordinator.Instance.ShowMetroDialogAsync(this, circularProgressBarDialog);           
        }

        public void PerfomStartGame(Client mainPlayer)
        {
            IsDrawer = LocalClient.Name == mainPlayer.Name;
        }


        public void PerfomEndGame()
        {
            Letters.Clear();
            RefreshClients(Clients.ToList());
            IsDrawer = true;
        }


        public void Ping()
        {
           return;
        }

        #endregion

        #region Static 

        public static ObservableCollection<char> MasckedCharList(int capacity,char mask)
        {
            ObservableCollection<char> list=new ObservableCollection<char>();
            for (int i = 0; i < capacity; i++)
            {
                list.Add(mask);
            }

            return list;
        }

        #endregion

        #region NotImplemented

        public IAsyncResult BeginPing(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndPing(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginWordChoose(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

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
        public IAsyncResult BeginReceiveLine(Line line, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceiveLine(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginRefreshLines(List<Line> line, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }
        public void EndRefreshLines(IAsyncResult result)
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

        public IAsyncResult BeginReciveLetter(char letter, int position, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReciveLetter(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginReciveWordInfo(int len, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReciveWordInfo(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginDrawerRequest(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndDrawerRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginWordChoose(List<string> words, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndWordChoose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginPerfomStartGame(Client mainPlayer,AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndPerfomStartGame(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginPerfomEndGame(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndPerfomEndGame(IAsyncResult result)
        {
            throw new NotImplementedException();
        }


        public IAsyncResult BeginLiveResponce(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public bool EndLiveResponce(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}