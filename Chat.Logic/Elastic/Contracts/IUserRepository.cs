using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IUserRepository
    {
        ElasticResult<User> Login(string login, string password);

        ElasticResult<User> CheckToken(string token);
    }
}