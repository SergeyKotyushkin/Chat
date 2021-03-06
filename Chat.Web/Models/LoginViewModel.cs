﻿namespace Chat.Web.Models
{
    public class LoginViewModel
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public MessageViewModel MessageViewModel { get; set; }


        public static LoginViewModel SuccessMessage(string message)
        {
            return new LoginViewModel
            {
                MessageViewModel = MessageViewModel.SuccessMessage(message)
            };
        }

        public static LoginViewModel ErrorMessage(string message)
        {
            return new LoginViewModel
            {
                MessageViewModel = MessageViewModel.ErrorMessage(message)
            };
        }
    }
}