using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCFMainServerAssembly
{
    [ServiceContract(CallbackContract = typeof(IServerCallback),
                          SessionMode = SessionMode.Allowed)]
    public interface IService
    {
        [OperationContract(IsOneWay = false)]
        bool Connect(Server server);
        [OperationContract(IsOneWay = true)]
        void Disconnect(Server server);
        [OperationContract(IsOneWay = false)]
        List<Server> GetAvalibleServers();       
    }

    public interface IServerCallback
    {
    }
}
