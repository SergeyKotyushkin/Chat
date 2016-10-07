using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IElasticRepository
    {
        string EsIndex { get; }

        ElasticResponse<T> ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class;

        ElasticResponse<T>[] ExecuteSearchRequestWithScroll<T>(SearchDescriptor<T> searchDescriptor) where T : class;

        ElasticIndexResponse ExecuteCreateOrUpdateRequest<T>(T @object, string esType) where T : class, IGuidedEntity;

        ElasticMultiGetResponse ExecuteMultiGetRequest(MultiGetDescriptor multiGetDescriptor);

        ElasticDeleteResponse ExecuteDeleteRequest<T>(DocumentPath<T> documentPath) where T : class;
    }
}