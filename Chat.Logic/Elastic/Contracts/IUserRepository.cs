using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IUserRepository
    {
        ElasticResult<ElasticUser> Login(string login, string password);

        ElasticResult<ElasticUser> CheckToken(string token);

        ElasticResult<ElasticUser> CheckLogin(string login);

        ElasticResult<ElasticUser> Add(string login, string password);

        ElasticResult<ElasticUser> Update(ElasticUser user);

        ElasticResult<ElasticUser[]> GetAll();

        ElasticResult<ElasticUser> Get(string guid);
    }
}