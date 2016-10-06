using System;
using System.Collections.Generic;
using System.Linq;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class ElasticRepository : IElasticRepository
    {
        private const string EsUri = "http://localhost:9200";
        private const string EsIndexName = "database";

        private const string ServerErrorMessage = "Server Error";


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
                return ElasticResponse<T>.FailResponse(ServerErrorMessage);
            }
        }

        public ElasticResponse<T>[] ExecuteSearchRequestWithScroll<T>(SearchDescriptor<T> searchDescriptor) where T : class
        {
            var scrollTime = new Time(2000);
            var searchDescriptorWithScroll = searchDescriptor.From(0).Size(10).Scroll(scrollTime);
            var results = new List<ElasticResponse<T>>();

            try
            {
                var client = GetElasticClient();
                var response = client.Search<T>(s => searchDescriptorWithScroll);
                if (!response.ApiCall.Success)
                    return
                        new[]
                        {
                            ElasticResponse<T>.FailResponse("Request ended with error: " +
                                                            response.ApiCall.OriginalException.Message)
                        };

                results.Add(ElasticResponse<T>.SuccessResponse(response));

                do
                {
                    var currentResponse = response;
                    response = client.Scroll<T>(scrollTime, currentResponse.ScrollId);
                    if (!response.ApiCall.Success)
                        return
                            new[]
                            {
                                ElasticResponse<T>.FailResponse("Request ended with error: " +
                                                                response.ApiCall.OriginalException.Message)
                            };

                    results.Add(ElasticResponse<T>.SuccessResponse(response));
                } while (response.IsValid && response.Documents.Any());

                return results.ToArray();
            }
            catch
            {
                return new[]
                {
                    ElasticResponse<T>.FailResponse(ServerErrorMessage)
                };
            }
        }

        public ElasticIndexResponse ExecuteCreateOrUpdateRequest<T>(T @object, string esType) where T : class, IGuidedEntity
        {
            try
            {
                var client = GetElasticClient();
                var response = client.Index(@object, i => i.Index(EsIndex).Type(esType).Id(@object.Guid));

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

                var response = client.CreateIndex(EsIndexName,
                    i =>
                        i.Mappings(
                            m => m
                                .Map<ElasticUser>(map => map.AutoMap())
                                .Map<ElasticChat>(map => map.AutoMap())
                                .Map<ElasticChatUser>(map => map.AutoMap())
                                .Map<ElasticMessage>(map => map.AutoMap())));

                return response.ApiCall.Success
                    ? ElasticResult<bool?>.SuccessResult(true)
                    : ElasticResult<bool?>.FailResult(response.ApiCall.ServerError.Error.ToString());
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