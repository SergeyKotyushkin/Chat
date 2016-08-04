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

            if (Request.Cookies[CookieName] != null)
                Response.Cookies.Add(new HttpCookie(CookieName) { Expires = DateTime.Now.AddDays(-1) });

            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Index(LoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Login) ||
                string.IsNullOrWhiteSpace(model.Password))
                return View(LoginViewModel.ErrorMessage("You should fill all the fields"));

            var login = model.Login.Trim();
            var password = model.Password.Trim();

            var loginResult = Login(login, password);
            if (!loginResult.Item1)
                return View(LoginViewModel.ErrorMessage("A user with these credentials does not exist"));

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