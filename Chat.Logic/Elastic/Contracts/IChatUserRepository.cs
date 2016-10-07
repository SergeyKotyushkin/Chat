using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IChatUserRepository
    {
        ElasticResult<ElasticChatUser> Get(string chatGuid, string userGuid);

        ElasticResult<ElasticChatUser> Add(string chatGuid, string userGuid);

        ElasticResult<bool> Remove(string guid);

        ElasticResult<bool> Remove(string chatGuid, string userGuid);

        ElasticResult<ElasticChatUser[]> GetAllByUserGuid(string userGuid);

        ElasticResult<ElasticChatUser[]> GetAllByChatGuid(string chatGuid);
    }
}