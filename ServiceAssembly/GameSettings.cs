using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Timers;

namespace ServiceAssembly
{
    [DataContract]
    public class GameSettings
    {
        [DataMember]
        public Client DrawingClient { get; set; }
        [DataMember]
        public string Word { get; set; }
        [DataMember]
        public List<int> Shownletters;
    }
}