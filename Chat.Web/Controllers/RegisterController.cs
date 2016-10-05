using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Chat.Logic.Elastic.Contracts;
using Chat.Web.Models;

namespace Chat.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly Regex loginRegex = new Regex(@"[^a-zA-Z0-9 ]+");
        private readonly IUserRepository _userRepository;


        public RegisterController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpGet]
        public ActionResult Index()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public ActionResult Index(RegisterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Login) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.PasswordRepeat))
                return View(RegisterViewModel.ErrorMessage("You should fill all the fields"));

            var login = model.Login.Trim();

            if(loginRegex.IsMatch(login))
                return View(RegisterViewModel.ErrorMessage("User Name may contain only letters, digits and white spaces"));

            var password = model.Password.Trim();
            var passwordRepeat = model.PasswordRepeat.Trim();

            if (!password.Equals(passwordRepeat))
                return View(RegisterViewModel.ErrorMessage("The passwords are not same"));

            if (CheckLogin(login))
                return View(RegisterViewModel.ErrorMessage("A user with the same login already exists"));

            var registerResult = CreateUser(login, password);
            return
                View(registerResult.Item1
                    ? RegisterViewModel.SuccessMessage("The registration has been performed with success")
                    : RegisterViewModel.ErrorMessage(registerResult.Item2));
        }


        private bool CheckLogin(string login)
        {
            var elasticResult = _userRepository.CheckLogin(login);

            return elasticResult.Success && elasticResult.Value != null;
        }

        private Tuple<bool, string> CreateUser(string login, string password)
        {
            var elasticResult = _userRepository.Add(login, password);

            var isSuccess = elasticResult.Success && elasticResult.Value != null;
            return new Tuple<bool, string>(isSuccess, isSuccess ? null : elasticResult.Message);
        }
    }
}
