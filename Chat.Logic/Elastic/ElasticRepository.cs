using System;
using System.Threading.Tasks;
using Chat.Logic.Elastic.Contracts;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class ElasticRepository : IElasticRepository
    {
        private const string EsUri = "http://localhost:9200";
        private const string EsIndexName = "database";

        #region Properties

        public string EsIndex
        {
            get { return EsIndexName; }
        }

        #endregion

        #region Public Methods

        public ElasticResponse<T> ExecuteSearchRequest<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            try
            {
                var client = GetElasticClient();
                var response = client.Search<T>(s => searchDescriptor);

                if (response.TimedOut)
                    return ElasticResponse<T>.FailResponse("The Request's Timeout was exceeded");

                return !response.ApiCall.Success
                    ? ElasticResponse<T>.FailResponse("Request ended with error: " +
                                                      response.ApiCall.OriginalException.Message)
                    : ElasticResponse<T>.SuccessResponse(response);
            }
            catch
            {
                return ElasticResponse<T>.FailResponse("Server error.");
            }
        }

        public ElasticIndexResponse ExecuteCreateOrUpdateRequest<T>(T @object, string esType) where T : class, IGuidedEntity
        {
            try
            {
                var client = GetElasticClient();
                var response = client.IndexAsync(@object, i => i.Index(EsIndex).Type(esType).Id(@object.Guid)).Result;

                return response.ApiCall.Success
                    ? ElasticIndexResponse.SuccessResponse(response)
                    : ElasticIndexResponse.FailResponse("Request ended with error. " +
                                                        response.ApiCall.OriginalException.Message);
            }
            catch
            {
                return ElasticIndexResponse.FailResponse("Server error.");
            }
        }

        public ElasticMultiGetResponse ExecuteMultiGetRequest(MultiGetDescriptor multiGetDescriptor)
        {
            try
            {
                var client = GetElasticClient();
                var response = client.MultiGet(m => multiGetDescriptor);

                return response.ApiCall.Success
                    ? ElasticMultiGetResponse.SuccessResponse(response)
                    : ElasticMultiGetResponse.FailResponse("Request ended with error. " +
                                                        response.ApiCall.OriginalException.Message);
            }
            catch
            {
                return ElasticMultiGetResponse.FailResponse("Server error");
            }
        }

        #endregion

        #region Public Static Methods

        public static ElasticResult<bool?> ElasticSearchCreateIndices()
        {
            try
            {
                var client = GetElasticClient();

                var isIndexExist = client.IndexExists(Indices.All, i => i.Index(EsIndexName)).Exists;
                if (isIndexExist)
                    return ElasticResult<bool?>.SuccessResult(true);

                //var response = client.CreateIndex(EsIndexName,
                //    i =>
                //        i.Mappings(
                //            m => m
                //                .Map<User>(map => map.AutoMap())
                //                .Map<Chat>(map => map.AutoMap())
                //                .Map<ChatUser>(map => map.AutoMap())));

                return ElasticResult<bool?>.SuccessResult(true);
            }
            catch
            {
                return ElasticResult<bool?>.FailResult("Server error.");
            }
        }

        #endregion


        #region Private Methods

        private static ElasticClient GetElasticClient()
        {
            var node = new Uri(EsUri);

            var settings = new ConnectionSettings(node);
            settings.RequestTimeout(TimeSpan.FromSeconds(3));

            return new ElasticClient(settings);
        }

        #endregion
    }
}