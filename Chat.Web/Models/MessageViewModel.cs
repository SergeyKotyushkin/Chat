namespace Chat.Web.Models
{
    public class MessageViewModel
    {
        public bool HasError { get; set; }

        public bool HasSuccess { get; set; }

        public string Message { get; set; }


        public static MessageViewModel SuccessMessage(string message)
        {
            return new MessageViewModel
            {
                HasError = false,
                HasSuccess = true,
                Message = message
            };
        }

        public static MessageViewModel ErrorMessage(string message)
        {
            return new MessageViewModel
            {
                HasError = true,
                HasSuccess = false,
                Message = message
            };
        }
    }
}