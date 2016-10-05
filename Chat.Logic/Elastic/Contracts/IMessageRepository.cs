using System;
using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IMessageRepository
    {
        ElasticResult<ElasticMessage> Add(string chatGuid, ElasticUser user, string text);

        ElasticResult<ElasticMessage> AddAdminMessage(string chatGuid, ElasticUser user, string newUserName);

        ElasticResult<ElasticMessage[]> GetAllByChatGuid(string guid, DateTime lastSendTime, int count);
    }
}