using System;
using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public Client Sender { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public string Time =>DateTime.Now.ToLongTimeString();

        public Message()
        {
        }

        public Message(Client sender, string content)
        {
            Sender = sender;
            Content = content;
        }
    }
}