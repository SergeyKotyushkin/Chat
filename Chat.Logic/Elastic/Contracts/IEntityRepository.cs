using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IEntityRepository
    {
        ElasticResult<T> Get<T>(string esType, string guid) where T : class, IGuidedEntity;

        ElasticResult<T> GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse<T> response) where T : class;
    }
}