using System;
using Chat.Logic.Elastic.Contracts;

namespace Chat.Logic.Elastic.Models
{
    public class ElasticMessage : IGuidedEntity
    {
        public ElasticMessage(string chatGuid, string userGuid, string userName, string text)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
            UserName = userName;
            SendTime = DateTime.Now;
            Text = text;
        }

        public string Guid { get; set; }

        public string ChatGuid { get; set; }

        public string UserGuid { get; set; }

        public string UserName { get; set; }

        public DateTime SendTime { get; set; }

        public string Text { get; set; }

        public string SendTimeDisplay
        {
            get { return SendTime.ToString("G"); }
        }
    }
}