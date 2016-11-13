using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceAssembly
{
    [ServiceContract]
    public interface IGameCallback
    {
        [OperationContract(IsOneWay = true)]
        void RefreshClients(List<Client> clients);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void ReceiveLine(Line line);

        [OperationContract(IsOneWay = true)]
        void RefreshLines(List<Line> line);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(Client client);

        [OperationContract(IsOneWay = true)]
        void UserJoin(Client client);

        [OperationContract(IsOneWay = true)]
        void UserLeave(Client client);

        [OperationContract(IsOneWay = true)]
        void ReciveLetter(char letter,int position);

        [OperationContract(IsOneWay = true)]
        void ReciveWordInfo(int len);

        [OperationContract(IsOneWay = true)]
        void DrawerRequest();

        [OperationContract(IsOneWay = true)]
        void WordChoose(List<string> words );

        [OperationContract(IsOneWay = true)]
        void PerfomStartGame();

        [OperationContract(IsOneWay = true)]
        void PerfomEndGame();

        [OperationContract(IsOneWay = true)]
        void Ping();
    }
}