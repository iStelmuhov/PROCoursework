using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace ServiceAssembly
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    IncludeExceptionDetailInFaults = true,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    UseSynchronizationContext = false)]
    public class ChatRoomService:IChat
    {
        Dictionary<Client, IChatCallback> _clients =
             new Dictionary<Client, IChatCallback>();

        List<Client> _clientList = new List<Client>();

        public IChatCallback CurrentCallback => OperationContext.Current.
            GetCallbackChannel<IChatCallback>();

        object _syncObj = new object();

        private bool SearchClientsByName(string name)
        {
            return _clients.Keys.FirstOrDefault(a => a.Name == name) != null;
        }

        public bool Connect(Client client)
        {
            if (!_clients.ContainsValue(CurrentCallback) &&
             !SearchClientsByName(client.Name))
            {
                lock (_syncObj)
                {
                    _clients.Add(client, CurrentCallback);
                    _clientList.Add(client);

                    foreach (Client key in _clients.Keys)
                    {
                        IChatCallback callback = _clients[key];
                        try
                        {
                            callback.RefreshClients(_clientList);
                            callback.UserJoin(client);
                        }
                        catch
                        {
                            _clients.Remove(key);
                            return false;
                        }

                    }

                }
                return true;
            }
            return false;
        }

        public void Say(Message msg)
        {
            lock (_syncObj)
            {
                foreach (IChatCallback callback in _clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Whisper(Message msg, Client receiver)
        {
            foreach (Client rec in _clients.Keys)
            {
                if (rec.Name == receiver.Name)
                {
                    IChatCallback callback = _clients[rec];
                    callback.ReceiveWhisper(msg, rec);

                    foreach (Client sender in _clients.Keys)
                    {
                        if (sender.Name == msg.Sender.Name)
                        {
                            IChatCallback senderCallback = _clients[sender];
                            senderCallback.ReceiveWhisper(msg, rec);
                            return;
                        }
                    }
                }
            }
        }

        public void IsWriting(Client client)
        {
            lock (_syncObj)
            {
                foreach (IChatCallback callback in _clients.Values)
                {
                    callback.IsWritingCallback(client);
                }
            }
        }

        public bool SendFile(FileMessage fileMsg, Client receiver)
        {
            foreach (Client rcvr in _clients.Keys)
            {
                if (rcvr.Name == receiver.Name)
                {
                    Message msg = new Message();
                    msg.Sender = fileMsg.Sender;
                    msg.Content = "I'M SENDING FILE.. " + fileMsg.FileName;

                    IChatCallback rcvrCallback = _clients[rcvr];
                    rcvrCallback.ReceiveWhisper(msg, receiver);
                    rcvrCallback.ReceiverFile(fileMsg, receiver);

                    foreach (Client sender in _clients.Keys)
                    {
                        if (sender.Name == fileMsg.Sender.Name)
                        {
                            IChatCallback sndrCallback = _clients[sender];
                            sndrCallback.ReceiveWhisper(msg, receiver);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Disconnect(Client client)
        {
            foreach (Client c in _clients.Keys)
            {
                if (client.Name == c.Name)
                {
                    lock (_syncObj)
                    {
                        this._clients.Remove(c);
                        this._clientList.Remove(c);
                        foreach (IChatCallback callback in _clients.Values)
                        {
                            callback.RefreshClients(this._clientList);
                            callback.UserLeave(client);
                        }
                    }
                    return;
                }
            }
        }
    }
}