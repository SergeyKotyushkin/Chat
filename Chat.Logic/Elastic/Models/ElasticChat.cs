using Chat.Logic.Elastic.Contracts;
using Nest;
using Newtonsoft.Json;

namespace Chat.Logic.Elastic.Models
{
    [ElasticsearchType]
    public class ElasticChat : IGuidedEntity
    {
        [JsonConstructor]
        public ElasticChat()
        {
        }

        public ElasticChat(string name, string creatorGuid, string adminGuid)
        {
            Guid = System.Guid.NewGuid().ToString();
            Name = name;
            CreatorGuid = creatorGuid;
            AdminGuid = adminGuid;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string CreatorGuid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string AdminGuid { get; set; }
    }
}