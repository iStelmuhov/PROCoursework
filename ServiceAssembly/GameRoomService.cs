using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ServiceAssembly
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
         IncludeExceptionDetailInFaults = true,
         ConcurrencyMode = ConcurrencyMode.Multiple,
         UseSynchronizationContext = false)]
    public class GameRoomService : IGame
    {

        private static readonly List<string> _log = new List<string>();

        Dictionary<Client, IGameCallback> _clients =
            new Dictionary<Client, IGameCallback>();

        List<Client> ClientList => _clients.Keys.ToList();

        List<Line> _lines = new List<Line>();

        public readonly Client Admin = new Client("Admin", new Picture("red", 'A'));

        IGameCallback CurrentCallback => OperationContext.Current.
            GetCallbackChannel<IGameCallback>();

        private CrocodileGame Game { get; set; }

        private readonly Timer _answerTimer;
        private bool _canSend = true;

        readonly object _syncObj = new object();
        readonly object _gameSync = new object();

        public GameRoomService()
        {
            Game = new CrocodileGame(this);
            _answerTimer = new Timer(5000);
            _answerTimer.Elapsed += _answerTimer_Elapsed;
        }

        private void _answerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Game.IsActive)
            {
                _canSend = true;
            }
        }

        private bool SearchClientsByName(string name)
        {
            return _clients.Keys.FirstOrDefault(a => a.Name == name) != null;
        }

        private Client SearchClietnsByName(string name)
        {
            return _clients.FirstOrDefault(p => p.Key.Name == name).Key;
        }

        private Client SearchClietnsByCallback(IGameCallback callback)
        {
            return _clients.FirstOrDefault(p => p.Value == callback).Key;
        }
        public bool Connect(Client client)
        {
            Monitor.Enter(_syncObj);
            if (_clients.ContainsValue(CurrentCallback) || SearchClientsByName(client.Name))
            {
                Monitor.Exit(_syncObj);
                return false;
            }

            client.Time = DateTime.Now;
            _clients.Add(client, CurrentCallback);
            (CurrentCallback as ICommunicationObject).Faulted += GameRoomService_Faulted;
            (CurrentCallback as ICommunicationObject).Closed += GameRoomService_Faulted;

                if (Game.IsActive)
                {
                    CurrentCallback.PerfomStartGame();
                    CurrentCallback.ReciveWordInfo(Game.Settings.Word.Length);
                    foreach (var letterPosition in Game.Settings.Shownletters)
                    {
                        CurrentCallback.ReciveLetter(Game.Settings.Word.ElementAt(letterPosition), letterPosition);
                    }
                }
               
                foreach (Client key in _clients.Keys)
                {
                    try
                    {
                        IGameCallback callback = _clients[key];
                        callback.RefreshClients(ClientList);
                        callback.RefreshLines(_lines);
                        callback.UserJoin(client);


                    }
                    catch
                    {
                        _clients.Remove(SearchClietnsByName(key.Name));
                        _clients.Remove(SearchClietnsByName(client.Name));
                        Monitor.Exit(_syncObj);
                    }
                }
                if (ClientList.Count >= 2)
                    CheckAndStarNewGame(true);

            Monitor.Exit(_syncObj);
            return true;
        }
        
        public void Disconnect(Client client)
        {
            if(client==null) return;

            foreach (Client c in _clients.Keys)
            {
                if (client.Name == c.Name)
                {
                    lock (_syncObj)
                    {
                        _clients.Remove(c);

                        foreach (IGameCallback callback in _clients.Values)
                        {
                            try
                            {
                                callback.RefreshClients(ClientList);
                                callback.UserLeave(client);
                            }
                            catch (Exception)
                            {
                                Disconnect(SearchClietnsByCallback(callback));
                            }
                        }

                        if (Game.IsActive && Game.Settings.DrawingClient.Equals(client))
                        {
                            Say(new Message(Admin,
                                $"Game ended!\n Drawer leave the game! \n Word:{Game.Settings.Word}\n"));
                            Game.EndGame();
                        }

                        if (ClientList.Count < 2)
                            Game.EndGame();
                    }
                    return;
                }
            }
        }

        private void GameRoomService_Faulted(object sender, EventArgs e)
        {
            Disconnect(SearchClietnsByCallback(sender as IGameCallback));
        }

        public void Say(Message msg)
        {
            lock (_syncObj)
            {
                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.Receive(msg);
                }
            }
            if (!Game.IsActive) return;
            if (!Equals(msg.Sender, Game.Settings.DrawingClient) && Game.CheckWord(msg.Content))
            {
                Game.EndGame(msg.Sender);
            }
        }

        public void SendLine(Line line)
        {
            if (Game.IsActive && !line.Sender.Equals(Game.Settings.DrawingClient))
                return;

            lock (_gameSync)
            {
                _lines.Add(line);
                foreach (IGameCallback callback in _clients.Values)
                {
                   callback.ReceiveLine(line);                
                }
            }
        }

        public void IsWriting(Client client)
        {
            lock (_syncObj)
            {
                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.IsWritingCallback(client);
                }
            }

        }

        public void StartNewGame()
        {
            _lines.Clear();
            lock (_gameSync)
            {

                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.Receive(new Message(Admin, $"New game started! \n {Game.Settings.DrawingClient.Name} is drawer!"));
                    callback.PerfomStartGame();
                    callback.RefreshLines(_lines);
                }
            }
        }

        public void EndGame()
        {
            lock (_gameSync)
            {

                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.PerfomEndGame();
                }
            }
            _canSend = true;
            CheckAndStarNewGame();
        }

        public void SendLetter(char letter, int position)
        {
            lock (_gameSync)
            {

                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.ReciveLetter(letter, position);
                }
            }
        }

        public void SendWordInfo(int len)
        {
            lock (_gameSync)
            {

                foreach (IGameCallback callback in _clients.Values)
                {
                    callback.ReciveWordInfo(len);
                }
            }
        }

        public void ReciveGameWord(string word, Client sender)
        {

            if (!string.IsNullOrEmpty(word))
            {
                if (!Game.IsActive && _clients.Count >= 2)
                {
                    Game.StarGame(new GameSettings() { DrawingClient = sender, Word = word, Shownletters = new List<int>() });
                }
            }
        }

        public void DrawerResponce(Client sender, bool answer)
        {
            if (!Game.IsActive && Game.Settings == null)
                if (!answer)
                {
                    _answerTimer.Stop();
                    _canSend = true;
                    CheckAndStarNewGame();
                    return;
                }
            _answerTimer.Stop();
            foreach (Client rec in _clients.Keys)
            {
                if (rec.Name == sender.Name)
                {
                    IGameCallback callback = _clients[rec];
                    try
                    {
                        callback.WordChoose(GetWords.FromTextFile("word_rus.txt",4,6));
                    }
                    catch (Exception ex)
                    {
                        _log.Add(ex.Message);
                    }
                    break;
                }
            }
        }

        private List<string> items = new List<string>() { "One", "Two", "Three" };
        private void CheckAndStarNewGame(bool isConnect = false)
        {
            if (ClientList.Count < 2) return;

            if (Game == null)
            {
                Game = new CrocodileGame(this);
            }

            if (Game.IsActive) return;

            Random rand = new Random();
            if (_canSend)
            {
                lock (_gameSync)
                {
                    try
                    {
                        _clients = _clients.OrderBy(x => x.Key.Time).ToDictionary(x => x.Key, x => x.Value);
                        if (isConnect)
                            _clients[ClientList[rand.Next(0, ClientList.Count - 1)]].DrawerRequest();
                        else
                            _clients[ClientList[rand.Next(0, ClientList.Count)]].DrawerRequest();
                        _canSend = false;
                        _answerTimer.Start();
                    }
                    catch (Exception ex)
                    {
                        _log.Add(ex.Message);
                    }
                }
            }
        }

    }
}