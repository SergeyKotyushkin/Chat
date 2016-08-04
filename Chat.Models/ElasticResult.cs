namespace Chat.Models
{
    public class ElasticResult<T>
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public T Value { get; private set; }

        public static ElasticResult<T> SuccessResult(T value)
        {
            return new ElasticResult<T> { Success = true, Value = value };
        }

        public static ElasticResult<T> FailResult(string errorMessage)
        {
            return new ElasticResult<T> { Success = false, Message = errorMessage };
        }
    }
}