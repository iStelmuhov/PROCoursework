using System.Collections.Generic;
using System.ServiceModel;

namespace ServiceAssembly
{
    [ServiceContract]
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void RefreshClients(List<Client> clients);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void ReceiveWhisper(Message msg, Client receiver);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(Client client);

        [OperationContract(IsOneWay = true)]
        void ReceiverFile(FileMessage msg, Client receiver);

        [OperationContract(IsOneWay = true)]
        void UserJoin(Client client);

        [OperationContract(IsOneWay = true)]
        void UserLeave(Client client);
    }
}