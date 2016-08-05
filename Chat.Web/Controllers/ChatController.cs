using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Newtonsoft.Json;

namespace Chat.Web.Controllers
{
    public class ChatController : Controller
    {
        private const string CookieName = "chat_secret";

        private readonly IUserRepository _userRepository;


        public ChatController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Close()
        {
            if (Request.Cookies[CookieName] != null)
                Response.Cookies.Add(new HttpCookie(CookieName) {Expires = DateTime.Now.AddDays(-1)});

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public string GetAllUsers(string guid)
        {
            // Check current user
            var userElasticResult = _userRepository.Get(guid);
            if (!userElasticResult.Success || userElasticResult.Value == null)
                return JsonConvert.SerializeObject(new User[] {});

            var elasticResult = _userRepository.GetAll();
            if (!elasticResult.Success)
                return JsonConvert.SerializeObject(new User[] {});

            var users = elasticResult.Value;
            return JsonConvert.SerializeObject(users.OrderBy(u => u.UserName));
        }
    }
}
