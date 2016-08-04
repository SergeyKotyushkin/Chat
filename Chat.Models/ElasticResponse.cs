using Nest;

namespace Chat.Models
{
    public class ElasticResponse<T> where T : class
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public ISearchResponse<T> Response { get; private set; }


        public static ElasticResponse<T> FailResponse(string errorMessage)
        {
            return new ElasticResponse<T> {Success = false, Message = errorMessage};
        }

        public static ElasticResponse<T> SuccessResponse(ISearchResponse<T> response)
        {
            return new ElasticResponse<T> { Success = true, Response = response };
        }
    }
}