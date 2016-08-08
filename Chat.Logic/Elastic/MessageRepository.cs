using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();

        private const string EsType = "message";

        public ElasticResult<ElasticMessage> Add(string chatGuid, ElasticUser user, string text)
        {
            var message = new ElasticMessage(chatGuid, user.Guid, user.UserName, text);

            return _entityRepository.Add(EsType, message);
        }

        public ElasticResult<ElasticMessage[]> GetAllByChatGuid(string guid)
        {
            var searchDescriptor = new SearchDescriptor<ElasticMessage>().Query(
                q => q.Term(t => t.Field(f => f.ChatGuid).Value(guid))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse(response);
        }
    }
}