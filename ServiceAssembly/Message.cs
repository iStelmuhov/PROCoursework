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
        public string Time { get; set; }

        public Message()
        {
            Time = DateTime.Now.ToLongTimeString();
        }

        public Message(Client sender, string content,string time)
        {
            Sender = sender;
            Content = content;
            Time = time;
        }
    }
}