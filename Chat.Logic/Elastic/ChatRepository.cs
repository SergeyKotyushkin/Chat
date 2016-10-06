using System.Linq;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class ChatRepository : IChatRepository
    {
        private const string EsType = "chat";

        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();


        public ElasticResult<ElasticChat> Add(string name, string creatorGuid)
        {
            var chat = new ElasticChat(name, creatorGuid);
            var response = CheckChat(chat);

            return !response.Success ? response : _entityRepository.Add(EsType, chat);
        }

        public ElasticResult<ElasticChat> Get(string chatGuid)
        {
            return _entityRepository.Get<ElasticChat>(EsType, chatGuid);
        }

        public ElasticResult<ElasticChat[]> GetByGuids(params string[] chatGuids)
        {
            return _entityRepository.GetAllByGuids<ElasticChat>(EsType, chatGuids);
        }

        #region Private Methods

        // Check Chat Is Unique
        private ElasticResult<ElasticChat> CheckChat(ElasticChat chat)
        {
            var searchDescriptor = new SearchDescriptor<ElasticChat>().Query(
                q => q.Term(t => t.Field(f => f.Name).Value(chat.Name))).Index(_elasticRepository.EsIndex).Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            // If request well executed. And user is unique.
            if (response.Success && !response.Response.Hits.Any())
                return ElasticResult<ElasticChat>.SuccessResult(chat);

            return response.Success
                ? ElasticResult<ElasticChat>.FailResult("Server Error")
                : ElasticResult<ElasticChat>.FailResult(response.Message);
        }

        #endregion
    }
}