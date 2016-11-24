using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

namespace WCFMainServerAssembly
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class ServerHostService : IService
    {
        Dictionary<Server, IServerCallback> _servers =
           new Dictionary<Server, IServerCallback>();

        List<Server> ServersList => _servers.Keys.ToList();

        IServerCallback CurrentCallback => OperationContext.Current.
           GetCallbackChannel<IServerCallback>();

        readonly object _syncObj = new object();
        private bool SearchServerByName(string name)
        {
            return _servers.Keys.FirstOrDefault(a => a.Name == name) != null;
        }

        private Server SearchServerByCallback(IServerCallback callback)
        {
            return _servers.FirstOrDefault(p => p.Value == callback).Key;
        }

        public bool Connect(Server server)
        {
            Monitor.Enter(_syncObj);
            if (!_servers.ContainsValue(CurrentCallback) &&
                !SearchServerByName(server.Name))
            {

               
                _servers.Add(server, CurrentCallback);
                (CurrentCallback as ICommunicationObject).Faulted += ServerHostService_Faulted;
                (CurrentCallback as ICommunicationObject).Closed += ServerHostService_Faulted;               
                Monitor.Exit(_syncObj);
                return true;
            }
            Monitor.Exit(_syncObj);
            return false;
        }

        private void ServerHostService_Faulted(object sender, EventArgs e)
        {
            Disconnect(SearchServerByCallback(sender as IServerCallback));
        }

        public void Disconnect(Server server)
        {
            if (server == null) return;

            foreach (Server c in _servers.Keys)
            {
                if (server.Name == c.Name)
                {
                    lock (_syncObj)
                    {
                        _servers.Remove(c);
                    }
                    return;
                }
            }
        }

        public List<Server> GetAvalibleServers()
        {
            return ServersList;
        }
    }
}
