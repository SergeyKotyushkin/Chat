using Chat.Logic.Elastic.Models;
using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IChatRepository
    {
        ElasticResult<ElasticChat> Add(string name, string creatorGuid);

        ElasticResult<ElasticChat[]> GetByGuids(params string[] chatGuids);
    }
}