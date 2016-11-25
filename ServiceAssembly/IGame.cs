using System.ServiceModel;

namespace ServiceAssembly
{
    [ServiceContract(CallbackContract = typeof(IGameCallback),
                         SessionMode = SessionMode.Required)]
    public interface IGame
    {
        [OperationContract(IsInitiating = true)]
        bool Connect(Client client);

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void SendLine(Line line);

        [OperationContract(IsOneWay = true)]
        void IsWriting(Client client);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(Client client);

        [OperationContract(IsOneWay = true)]
        void StartNewGame();

        [OperationContract(IsOneWay = true)]
        void EndGame();

        [OperationContract(IsOneWay = true)]
        void SendLetter(char letter,int position);

        [OperationContract(IsOneWay = true)]
        void SendWordInfo(int len);

        [OperationContract(IsOneWay = true)]
        void ReciveGameWord(string word,Client sender);

        [OperationContract(IsOneWay = true)]
        void DrawerResponce(Client sender,bool answer);

        [OperationContract(IsOneWay = true)]
        void ClearLines(Client sender);

    }
}