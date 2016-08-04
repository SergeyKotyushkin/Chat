using Nest;

namespace Chat.Models
{
    public class ElasticIndexResponse
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public IIndexResponse Response { get; private set; }


        public static ElasticIndexResponse FailResponse(string errorMessage)
        {
            return new ElasticIndexResponse { Success = false, Message = errorMessage };
        }

        public static ElasticIndexResponse SuccessResponse(IIndexResponse response)
        {
            return new ElasticIndexResponse { Success = true, Response = response };
        } 
    }
}