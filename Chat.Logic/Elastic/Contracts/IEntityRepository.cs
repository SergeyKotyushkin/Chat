using Chat.Models;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IEntityRepository
    {
        ElasticResult<T> Get<T>(string esType, string guid) where T : class, IGuidedEntity;

        ElasticResult<T> GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse<T> response) where T : class;

        ElasticResult<T> Add<T>(string esType, T @object) where T : class, IGuidedEntity;

        ElasticResult<bool> Remove<T>(string esType, string guid) where T : class;

        ElasticResult<T[]> GetAll<T>(string esType) where T : class;

        ElasticResult<T[]> GetEntitiesFromElasticResponse<T>(ElasticResponse<T> response) where T : class;

        ElasticResult<T[]> GetEntitiesFromElasticResponseWithScroll<T>(ElasticResponse<T>[] responses) where T : class;

        ElasticResult<T[]> GetAllByGuids<T>(string esType, params string[] guids) where T : class;
    }
}