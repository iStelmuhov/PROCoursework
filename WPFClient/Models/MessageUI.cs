using System;

namespace WPFClient.Models
{
    public class MessageUI
    {

        public string Time => DateTime.Now.ToLongTimeString();
        public ClientUI User { get; }
        public string Text { get; }

        public MessageUI( ClientUI user, string text)
        {
            User = user;
            Text = text;
        }

        public MessageUI(SVC.Client user, string text)
        {
            User = new ClientUI(user.Name,user.Pic.Color);
            Text = text;
        }

    }
}