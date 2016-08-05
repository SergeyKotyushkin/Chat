using Nest;

namespace Chat.Models
{
    public class ElasticMultiGetResponse
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public IMultiGetResponse Response { get; private set; }


        public static ElasticMultiGetResponse FailResponse(string errorMessage)
        {
            return new ElasticMultiGetResponse { Success = false, Message = errorMessage };
        }

        public static ElasticMultiGetResponse SuccessResponse(IMultiGetResponse response)
        {
            return new ElasticMultiGetResponse { Success = true, Response = response };
        } 
    }
}