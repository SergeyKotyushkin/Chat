using System.Linq;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class EntityRepository : IEntityRepository
    {
        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();

        #region Public Methods

        public ElasticResult<T> Get<T>(string esType, string guid) where T : class, IGuidedEntity
        {
            var searchDescriptor = new SearchDescriptor<T>().Query(
                q => q.Term(t => t.Field(f => f.Guid).Value(guid))).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return GetEntityIfOnlyOneEntityInElasticResponse(response);
        }

        public ElasticResult<T> GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse<T> response)
            where T : class
        {
            // If request bad executed.
            if (!response.Success)
                return ElasticResult<T>.FailResult(response.Message);

            var hits = response.Response.Hits;
            var hitsArray = hits as IHit<T>[] ?? hits.ToArray();

            // If found not 1 entity
            if (response.Success && hitsArray.Count() != 1)
                return ElasticResult<T>.FailResult(null);

            return ElasticResult<T>.SuccessResult(hitsArray.ElementAt(0).Source);
        }

        public ElasticResult<T> Add<T>(string esType, T @object) where T : class, IGuidedEntity
        {
            var response = _elasticRepository.ExecuteCreateOrUpdateRequest(@object, esType);

            return response.Success
                ? ElasticResult<T>.SuccessResult(@object)
                : ElasticResult<T>.FailResult(response.Message);
        }

        public ElasticResult<T[]> GetAll<T>(string esType) where T : class
        {
            var searchDescriptor = new SearchDescriptor<T>().AllIndices().Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return GetEntitiesFromElasticResponse(response);
        }

        public ElasticResult<T[]> GetEntitiesFromElasticResponse<T>(ElasticResponse<T> response) where T : class
        {
            // If request bad executed.
            return !response.Success
                ? ElasticResult<T[]>.FailResult(response.Message)
                : ElasticResult<T[]>.SuccessResult(
                    response.Response.Hits.Select(h => h.Source).Where(s => s != null).ToArray());
        }

        public ElasticResult<T[]> GetByGuids<T>(string esType, params string[] guids) where T : class
        {
            if (guids.Length == 0)
                return ElasticResult<T[]>.SuccessResult(new T[] {});

            var multiGetDescriptor =
                new MultiGetDescriptor().GetMany<T>(guids).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteMultiGetRequest(multiGetDescriptor);

            if (!response.Success)
                return ElasticResult<T[]>.FailResult(response.Message);

            var multiGetResponse = response.Response;
            var hits = multiGetResponse.GetMany<T>(guids);

            return ElasticResult<T[]>.SuccessResult(hits.Select(hit => hit.Source).Where(hit => hit != null).ToArray());
        }

        #endregion
    }
}