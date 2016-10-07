using Nest;

namespace Chat.Models
{
    public class ElasticDeleteResponse
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public IDeleteResponse Response { get; private set; }
        
        public static ElasticDeleteResponse FailResponse(string errorMessage)
        {
            return new ElasticDeleteResponse { Success = false, Message = errorMessage };
        }

        public static ElasticDeleteResponse SuccessResponse(IDeleteResponse response)
        {
            return new ElasticDeleteResponse { Success = true, Response = response };
        } 
    }
}
