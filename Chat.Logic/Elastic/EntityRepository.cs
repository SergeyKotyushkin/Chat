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


        public ElasticResult<T> Get<T>(string esType, string guid) where T : class, IGuidedEntity
        {
            var searchDescriptor = new SearchDescriptor<T>().Query(
                q => q.Term(t => t.Field(f => f.Guid).Value(guid))).Index(_elasticRepository.EsIndex).Type(esType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return GetEntityIfOnlyOneEntityInElasticResponse(response);
        }

        public ElasticResult<T> GetEntityIfOnlyOneEntityInElasticResponse<T>(ElasticResponse<T> response) where T : class
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
    }
}