using Chat.Logic.Elastic.Contracts;
using Nest;

namespace Chat.Logic.Elastic.Models
{
    public class ElasticChat : IGuidedEntity
    {
        public ElasticChat(string name, string creatorGuid)
        {
            Guid = System.Guid.NewGuid().ToString();
            Name = name;
            CreatorGuid = creatorGuid;
        }

        public ElasticChat(string guid, string name, string creatorGuid)
        {
            Guid = guid;
            Name = name;
            CreatorGuid = creatorGuid;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string CreatorGuid { get; set; }
    }
}