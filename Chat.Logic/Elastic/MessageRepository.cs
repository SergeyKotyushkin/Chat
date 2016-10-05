using System;
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
        private const string NewUserInChatFormatString = "New User {0} has connected to this chat";

        public ElasticResult<ElasticMessage> Add(string chatGuid, ElasticUser user, string text)
        {
            var message = new ElasticMessage(chatGuid, user.Guid, user.UserName, text);

            return _entityRepository.Add(EsType, message);
        }

        public ElasticResult<ElasticMessage> AddAdminMessage(string chatGuid, ElasticUser user, string newUserName)
        {
            var message = new ElasticMessage(chatGuid, user.Guid, user.UserName,
                string.Format(NewUserInChatFormatString, newUserName), true);

            return _entityRepository.Add(EsType, message);

            throw new NotImplementedException();
        }

        public ElasticResult<ElasticMessage[]> GetAllByChatGuid(string guid, DateTime lastSendTime, int count)
        {
            var searchDescriptor = new SearchDescriptor<ElasticMessage>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m =>
                                    m.Term(fields => fields.Field(f => f.ChatGuid).Value(guid)) &&
                                    m.DateRange(fields => fields.Field(f => f.SendTime).LessThan(lastSendTime)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType)
                .Size(count)
                .Sort(s => s.Field(sf => sf.Field(f => f.SendTime).Descending()));

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse(response);
        }
    }
}