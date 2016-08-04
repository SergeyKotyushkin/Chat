using System;
using System.Web;
using System.Web.Mvc;
using Chat.Logic.Elastic.Contracts;
using Chat.Web.Models;

namespace Chat.Web.Controllers
{
    public class LoginController : Controller
    {
        private const string CookieName = "chat_secret";

        private readonly IUserRepository _userRepository;

        
        public LoginController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpGet]
        public ActionResult Index()
        {
            if (CheckCookies(Request.Cookies))
                return RedirectToAction("Index", "Chat");

            Response.Cookies.Remove(CookieName);
            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Index(string login, string password)
        {
            if (login.Trim().Equals(string.Empty) ||
                password.Trim().Equals(string.Empty))
                return View(new LoginViewModel
                {
                    HasError = true, 
                    ErrorMessage = "The 'Login' and 'Password' fields must be are entered"
                });


            var loginResult = Login(login, password);
            if (!loginResult.Item1)
                return View(new LoginViewModel
                {
                    HasError = true,
                    ErrorMessage = "The user with these credentials is not exist"
                });

            Response.Cookies.Set(new HttpCookie(CookieName, loginResult.Item2));
            return RedirectToAction("Index", "Chat");
        }


        private bool CheckCookies(HttpCookieCollection cookies)
        {
            return cookies[CookieName] != null && CheckToken(cookies[CookieName].Value);
        }

        private bool CheckToken(string token)
        {
            var elasticResult = _userRepository.CheckToken(token);

            return elasticResult.Success && elasticResult.Value != null;
        }
        
        private Tuple<bool, string> Login(string login, string password)
        {
            var elasticResult = _userRepository.Login(login, password);

            var isExist = elasticResult.Success && elasticResult.Value != null;
            return new Tuple<bool, string>(isExist, isExist ? elasticResult.Value.Token : null);
        }
    }
}