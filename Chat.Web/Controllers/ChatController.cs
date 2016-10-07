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
        private readonly IMessageRepository _messageRepository;


        public ChatController(IUserRepository userRepository, IChatRepository chatRepository,
            IChatUserRepository chatUserRepository, IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _chatUserRepository = chatUserRepository;
            _messageRepository = messageRepository;
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
        public string GetMessages(string userGuid, string chatGuid, string lastSendDateString, int count)
        {
            // Check current user
            var userElasticResult = _userRepository.Get(userGuid);
            if (!userElasticResult.Success || userElasticResult.Value == null)
                return JsonConvert.SerializeObject(new { error = true, message = ReloginMessage });

            DateTime lastSendDate;
            if (string.IsNullOrEmpty(lastSendDateString) || !DateTime.TryParse(lastSendDateString, out lastSendDate))
                lastSendDate = DateTime.Now;
            var messageslasticResult = _messageRepository.GetAllByChatGuid(chatGuid, lastSendDate, count);

            return !messageslasticResult.Success
                ? JsonConvert.SerializeObject(new List<ElasticChat>())
                : JsonConvert.SerializeObject(messageslasticResult.Value.OrderByDescending(u => u.SendTime));
        }

        [HttpPost]
        public string GetAllChatUsersForChat(string chatGuid)
        {
            var chatUserlasticResult = _chatUserRepository.GetAllByChatGuid(chatGuid);
            if (!chatUserlasticResult.Success)
                return JsonConvert.SerializeObject(new List<ElasticChat>());

            var userElasticResult =
                _userRepository.GetAllByGuids(chatUserlasticResult.Value.Select(c => c.UserGuid).ToArray());
            if (!userElasticResult.Success)
                return JsonConvert.SerializeObject(new List<ElasticChat>());

            return JsonConvert.SerializeObject(userElasticResult.Value);
        }

        #endregion
    }
}
