using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Chat.Web
{
    public class ChatHub : Hub
    {
        private const string CookieName = "chat_secret";
        private const string ServerErrorMessage = "Server Error";

        private readonly IUserRepository _userRepository;
        private readonly IChatUserRepository _chatUserRepository;


        public ChatHub(IUserRepository userRepository, IChatUserRepository chatUserRepository)
        {
            _userRepository = userRepository;
            _chatUserRepository = chatUserRepository;
        }

        #region Public Methods

        public void Login()
        {
            // Get user by cookie
            var user = GetUserFromCookies(Context.RequestCookies);
            if (user == null)
            {
                Clients.Caller.onLoginCaller(JsonConvert.SerializeObject(new
                {
                    error = true,
                    message = "Please relogin to chat. Your personal data is incorrect."
                }));

                return;
            }

            // Update connection ids
            user.ConnectionIds.Add(Context.ConnectionId);
            var elasticResult = _userRepository.Update(user);
            if (!elasticResult.Success || elasticResult.Value == null)
            {
                Clients.Caller.onLoginCaller(
                    JsonConvert.SerializeObject(new {error = true, message = ServerErrorMessage}));
                return;
            }

            #region Add current connection to all user's chats
            
            var chatUserElasticResult = _chatUserRepository.GetAllByUserGuid(user.Guid);
            if (!elasticResult.Success || elasticResult.Value == null)
            {
                Clients.Caller.onLoginCaller(
                    JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                return;
            }
            
            foreach (var chatUser in chatUserElasticResult.Value)
                Groups.Add(Context.ConnectionId, chatUser.ChatGuid);
            
            #endregion

            user = elasticResult.Value;
            if (user.ConnectionIds.Count() == 1)
                Clients.Others.onLoginOthers(JsonConvert.SerializeObject(user));
            Clients.Caller.onLoginCaller(JsonConvert.SerializeObject(user));
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Get user by cookie
            var user = GetUserFromCookies(Context.RequestCookies);
            if (user == null)
                return base.OnDisconnected(stopCalled);

            // Remove current connection id
            user.ConnectionIds.Remove(Context.ConnectionId);
            //user.ConnectionIds.Clear();
            var elasticResult = _userRepository.Update(user);
            if(!elasticResult.Success || elasticResult.Value == null)
                return base.OnDisconnected(stopCalled);
            
            #region Remove current connection from all user's chats

            var chatUserElasticResult = _chatUserRepository.GetAllByUserGuid(user.Guid);
            if (!elasticResult.Success || elasticResult.Value == null)
                return base.OnDisconnected(stopCalled);

            foreach (var chatUser in chatUserElasticResult.Value)
                Groups.Remove(Context.ConnectionId, chatUser.ChatGuid);

            #endregion

            // Alert others if you became offline
            var updatedUser = elasticResult.Value;
            if (!updatedUser.IsOnline)
                Clients.All.onDisconnectOthers(JsonConvert.SerializeObject(updatedUser));

            return base.OnDisconnected(stopCalled);
        }

        public void CreateChat(string name, string guid)
        {
            var userElasticResult = _userRepository.Get(guid);
            if(!userElasticResult.Success)
                return;

            // Add all the connection ids of the user to the new chat
            foreach (var connectionId in userElasticResult.Value.ConnectionIds)
                Groups.Add(connectionId, name);
        }

        #endregion

        #region Private Methods

        private User GetUserFromCookies(IDictionary<string, Cookie> cookies)
        {
            if (!cookies.ContainsKey(CookieName))
                return null;

            var elasticResult = _userRepository.CheckToken(cookies[CookieName].Value);
            return elasticResult.Success && elasticResult.Value != null ? elasticResult.Value : null;
        }

        #endregion
    }
}