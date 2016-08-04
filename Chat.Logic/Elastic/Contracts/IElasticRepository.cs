using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic.Contracts
{
    public interface IElasticRepository
    {
        string EsIndex { get; }

        ElasticResponse<T> ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class;
    }
}