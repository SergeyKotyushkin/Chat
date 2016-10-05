using System;
using Chat.Logic.Elastic.Contracts;
using Newtonsoft.Json;

namespace Chat.Logic.Elastic.Models
{
    public class ElasticMessage : IGuidedEntity
    {
        [JsonConstructor]
        public ElasticMessage()
        {
            
        }

        public ElasticMessage(string chatGuid, string userGuid, string userName, string text)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
            UserName = userName;
            SendTime = DateTime.Now;
            Text = text;
            IsAdminMessage = false;
        }

        public ElasticMessage(string chatGuid, string userGuid, string userName, string text, bool isAdminMessage)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
            UserName = userName;
            SendTime = DateTime.Now;
            Text = text;
            IsAdminMessage = isAdminMessage;
        }

        public string Guid { get; set; }

        public string ChatGuid { get; set; }

        public string UserGuid { get; set; }

        public string UserName { get; set; }

        public DateTime SendTime { get; set; }

        public string Text { get; set; }

        public bool IsAdminMessage { get; set; }

        public string SendTimeDisplay
        {
            get { return SendTime.ToString("G"); }
        }
    }
}