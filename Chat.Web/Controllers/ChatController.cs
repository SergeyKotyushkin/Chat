using System;
using System.Collections.Generic;
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
        private const string ReloginMessage = "Please relogin to chat. Your personal data is incorrect.";
        private const string FillChatNameMessage = "Fill name of the new chat";

        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatUserRepository _chatUserRepository;


        public ChatController(IUserRepository userRepository, IChatRepository chatRepository,
            IChatUserRepository chatUserRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _chatUserRepository = chatUserRepository;
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

        #region Ajax Requests

        [HttpPost]
        public string GetAllUsers(string guid)
        {
            // Check current user
            var userElasticResult = _userRepository.Get(guid);
            if (!userElasticResult.Success || userElasticResult.Value == null)
                return JsonConvert.SerializeObject(new {error = true, message = ReloginMessage});

            var elasticResult = _userRepository.GetAll();
            if (!elasticResult.Success)
                return JsonConvert.SerializeObject(new ElasticUser[] {});

            var users = elasticResult.Value;
            return JsonConvert.SerializeObject(users.OrderBy(u => u.UserName));
        }

        [HttpPost]
        public string GetAllChats(string guid)
        {
            // Check current user
            var userElasticResult = _userRepository.Get(guid);
            if (!userElasticResult.Success || userElasticResult.Value == null)
                return JsonConvert.SerializeObject(new {error = true, message = ReloginMessage});

            var chatUserlasticResult = _chatUserRepository.GetAllByUserGuid(guid);
            if (!chatUserlasticResult.Success)
                return JsonConvert.SerializeObject(new List<ElasticChat>());

            var chatElasticResult =
                _chatRepository.GetByGuids(chatUserlasticResult.Value.Select(c => c.ChatGuid).ToArray());
            if (!chatElasticResult.Success)
                return JsonConvert.SerializeObject(new List<ElasticChat>());

            return JsonConvert.SerializeObject(chatElasticResult.Value.OrderBy(u => u.Name));
        }

        [HttpPost]
        public string CreateChat(string name, string guid)
        {
            if (string.IsNullOrWhiteSpace(name))
                return JsonConvert.SerializeObject(new {error = true, code = 0, message = FillChatNameMessage});

            // Check current user
            var userElasticResult = _userRepository.Get(guid);
            if (!userElasticResult.Success || userElasticResult.Value == null)
                return JsonConvert.SerializeObject(new {error = true, code = 1, message = ReloginMessage});

            var chatElasticResult = _chatRepository.Add(name, guid);
            if (!chatElasticResult.Success)
                return JsonConvert.SerializeObject(new {error = true, code = 2, message = chatElasticResult.Message});

            var chatUserElasticResult = _chatUserRepository.Add(chatElasticResult.Value.Guid, guid);
            if (!chatUserElasticResult.Success)
                return JsonConvert.SerializeObject(new {error = true, code = 2, message = chatUserElasticResult.Message});

            return JsonConvert.SerializeObject(new {error = false});
        }

        #endregion
    }
}
