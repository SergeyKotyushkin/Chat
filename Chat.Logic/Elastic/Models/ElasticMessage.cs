using System;
using Chat.Logic.Elastic.Contracts;
using Nest;
using Newtonsoft.Json;

namespace Chat.Logic.Elastic.Models
{
    [ElasticsearchType]
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

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string ChatGuid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserGuid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public DateTime SendTime { get; set; }

        [String(Index = FieldIndexOption.Analyzed)]
        public string Text { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public bool IsAdminMessage { get; set; }

        [String(Index = FieldIndexOption.Analyzed)]
        public string SendTimeDisplay
        {
            get { return SendTime.ToString("G"); }
        }
    }
}