namespace Chat.Web.Models
{
    public class RegisterViewModel
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public string PasswordRepeat { get; set; }

        public MessageViewModel MessageViewModel { get; set; }


        public static RegisterViewModel SuccessMessage(string message)
        {
            return new RegisterViewModel
            {
                MessageViewModel = MessageViewModel.SuccessMessage(message)
            };
        }

        public static RegisterViewModel ErrorMessage(string message)
        {
            return new RegisterViewModel
            {
                MessageViewModel = MessageViewModel.ErrorMessage(message)
            };
        }
    }
}