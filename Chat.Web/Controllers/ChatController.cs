using System;
using System.Web;
using System.Web.Mvc;

namespace Chat.Web.Controllers
{
    public class ChatController : Controller
    {
        private const string CookieName = "chat_secret";

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Close()
        {
            if (Request.Cookies[CookieName] != null)
                Response.Cookies.Add(new HttpCookie(CookieName) { Expires = DateTime.Now.AddDays(-1) });

            return RedirectToAction("Index", "Login");
        }
    }
}
