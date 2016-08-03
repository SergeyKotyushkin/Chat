using System.Web;
using System.Web.Mvc;
using Chat.Web.Models;

namespace Chat.Web.Controllers
{
    public class LoginController : Controller
    {
        private const string CookieName = "chat_secret";

        [HttpGet]
        public ActionResult Index()
        {
            if (CheckCookies(Request.Cookies))
            {
                // TODO: Redirect to chat page
            }


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

            return View(new LoginViewModel());
        }


        private static bool CheckCookies(HttpCookieCollection cookies)
        {
            return cookies[CookieName] != null && CheckUser(cookies[CookieName].Value);
        }

        private static bool CheckUser(string secret)
        {
            return false;
        }
    }
}