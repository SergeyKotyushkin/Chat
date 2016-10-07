using System.Linq;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class ChatUserRepository : IChatUserRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();

        private const string EsType = "chatuser";
        private const string ServerErrorMessage = "Server Error";
        private const string InvalidDataMessage = "Invalid data";

        #region Public Methods

        public ElasticResult<ElasticChatUser> Get(string chatGuid, string userGuid)
        {
            var searchDescriptor =
                new SearchDescriptor<ElasticChatUser>().Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.UserGuid).Value(userGuid)),
                            m => m.Term(t => t.Field(f => f.ChatGuid).Value(chatGuid))
                        ))
                    )
                    .Index(_elasticRepository.EsIndex)
                    .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return response.Success
                ? _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse(response)
                : ElasticResult<ElasticChatUser>.FailResult(response.Message);
        }

        public ElasticResult<ElasticChatUser> Add(string chatGuid, string userGuid)
        {
            var chatUser = new ElasticChatUser(chatGuid, userGuid);
            var response = CheckChatUser(chatUser);

            return !response.Success ? response : _entityRepository.Add(EsType, chatUser);
        }

        public ElasticResult<bool> Remove(string guid)
        {
            return _entityRepository.Remove<ElasticChatUser>(EsType, guid);
        }

        public ElasticResult<bool> Remove(string chatGuid, string userGuid)
        {
            var chatUserResponse = Get(chatGuid, userGuid);
            return !chatUserResponse.Success
                ? ElasticResult<bool>.FailResult(InvalidDataMessage)
                : _entityRepository.Remove<ElasticChatUser>(EsType, chatUserResponse.Value.Guid);
        }

        public ElasticResult<ElasticChatUser[]> GetAllByUserGuid(string userGuid)
        {
            var searchDescriptor =
                new SearchDescriptor<ElasticChatUser>().Query(q => q.Term(t => t.Field(f => f.UserGuid).Value(userGuid)))
                    .Index(_elasticRepository.EsIndex)
                    .Type(EsType);

            var responses = _elasticRepository.ExecuteSearchRequestWithScroll(searchDescriptor);

            return responses.Any(r => !r.Success)
                ? ElasticResult<ElasticChatUser[]>.FailResult(responses.First(r => !r.Success).Message)
                : _entityRepository.GetEntitiesFromElasticResponseWithScroll(responses);
        }

        public ElasticResult<ElasticChatUser[]> GetAllByChatGuid(string chatGuid)
        {
            var searchDescriptor = new SearchDescriptor<ElasticChatUser>()
                .Query(q => q.Term(t => t.Field(f => f.ChatGuid).Value(chatGuid)))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var responses = _elasticRepository.ExecuteSearchRequestWithScroll(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponseWithScroll(responses);
        }

        #endregion


        #region Private Methods

        private ElasticResult<ElasticChatUser> CheckChatUser(ElasticChatUser chatUser)
        {
            var searchDescriptor = new SearchDescriptor<ElasticChatUser>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m => m.Term(t => t.Field(f => f.ChatGuid).Value(chatUser.ChatGuid)),
                                m => m.Term(t => t.Field(f => f.UserGuid).Value(chatUser.UserGuid)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            // If request well executed. And user is unique.
            if (response.Success && !response.Response.Hits.Any())
                return ElasticResult<ElasticChatUser>.SuccessResult(chatUser);

            return response.Success
                ? ElasticResult<ElasticChatUser>.FailResult("User in this chat already!")
                : ElasticResult<ElasticChatUser>.FailResult(response.Message);
        }

        #endregion
    }
}