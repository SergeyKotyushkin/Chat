using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IUserRepository
    {
        ElasticResult<User> Login(string login, string password);

        ElasticResult<User> CheckToken(string token);

        ElasticResult<User> CheckLogin(string login);

        ElasticResult<User> Add(string login, string password);

        ElasticResult<User> Update(User user);

        ElasticResult<User[]> GetAll();

        ElasticResult<User> Get(string guid);
    }
}