using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IChatUserRepository
    {
        ElasticResult<ElasticChatUser> Add(string chatGuid, string userGuid);

        ElasticResult<ElasticChatUser[]> GetAllByUserGuid(string userGuid);
    }
}