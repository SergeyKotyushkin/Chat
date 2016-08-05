using Chat.Logic.Elastic.Contracts;
using Nest;

namespace Chat.Logic.Elastic.Models
{
    public class ElasticChatUser : IGuidedEntity
    {
        public ElasticChatUser(string chatGuid, string userGuid)
        {
            Guid = System.Guid.NewGuid().ToString();
            ChatGuid = chatGuid;
            UserGuid = userGuid;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string ChatGuid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserGuid { get; set; }
    }
}