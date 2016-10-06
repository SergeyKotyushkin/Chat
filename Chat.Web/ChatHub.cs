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
        private const string UserErrorMessage = "Please relogin to chat. Your personal data is incorrect.";
        private const string ChatErrorMessage = "Invalid chat! Please refresh the page";

        private readonly IUserRepository _userRepository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRepository _chatRepository;


        public ChatHub(IUserRepository userRepository, IChatUserRepository chatUserRepository,
            IMessageRepository messageRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatUserRepository = chatUserRepository;
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
        }

        #region Public Methods

        public void Login()
        {
            // Get user by cookie
            var user = GetUserFromCookies(Context.RequestCookies);
            if (user == null)
            {
                Clients.Caller.onLoginCaller(JsonConvert.SerializeObject(new {error = true, message = UserErrorMessage}));
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
            if (!userElasticResult.Success)
            {
                Clients.Caller.onCreateChatCaller(
                    JsonConvert.SerializeObject(new { error = true, message = UserErrorMessage }));
                return;
            }

            var user = userElasticResult.Value;

            var chatElasticResult = _chatRepository.Add(name, guid);
            if (!chatElasticResult.Success)
            {
                Clients.Caller.onCreateChatCaller(
                    JsonConvert.SerializeObject(new { error = true, message = chatElasticResult.Message }));
                return;
            }

            var chat = chatElasticResult.Value;

            // Add User to new Chat
            var chatUserElasticResult = _chatUserRepository.Add(chat.Guid, user.Guid);
            if (!chatUserElasticResult.Success)
            {
                Clients.Caller.OnCreateChat(
                    JsonConvert.SerializeObject(new {error = true, message = chatUserElasticResult.Message}));
                return;
            }

            // Add all the connection ids of the user to the new chat
            foreach (var connectionId in user.ConnectionIds)
                Groups.Add(connectionId, chat.Guid);

            Clients.Clients(user.ConnectionIds.ToArray()).OnCreateChatCaller(JsonConvert.SerializeObject(chat));
        }

        public void SendMessage(string text, string userGuid, string chatGuid)
        {
            var userElasticResult = _userRepository.Get(userGuid);
            if (!userElasticResult.Success)
            {
                Clients.Caller.onSendMessageCaller(
                    JsonConvert.SerializeObject(new { error = true, message = UserErrorMessage }));
                return;
            }

            var user = userElasticResult.Value;

            // Save Message
            var messageElasticResult = _messageRepository.Add(chatGuid, user, text);
            if (!messageElasticResult.Success)
            {
                Clients.Caller.onSendMessageCaller(
                    JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                return;
            }

            Clients.Group(chatGuid)
                .onSendMessageOthers(JsonConvert.SerializeObject(new {message = messageElasticResult.Value, chatGuid}));
        }

        public void SearchUsersForAddUsersToChat(string userName, string userGuid, string chatGuid)
        {
            var userElasticResult = _userRepository.Get(userGuid);
            if (!userElasticResult.Success)
            {
                Clients.Caller.onSearchUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new {error = true, message = UserErrorMessage}));
                return;
            }

            var chatElasticResult = _chatRepository.Get(chatGuid);
            if (!chatElasticResult.Success)
            {
                Clients.Caller.onSearchUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new {error = true, message = ChatErrorMessage}));
                return;
            }

            var searchElasticResult = _userRepository.SearchByUserName(userName);
            if (!searchElasticResult.Success)
            {
                Clients.Caller.onSearchUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new {error = true, message = ServerErrorMessage}));
                return;
            }

            var users = searchElasticResult.Value;

            var chatUserElasticResult = _chatUserRepository.GetAllByChatGuid(chatGuid);
            if (!chatUserElasticResult.Success)
            {
                Clients.Caller.onSearchUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new {error = true, message = ServerErrorMessage}));
                return;
            }

            var chatUsers = chatUserElasticResult.Value;

            var searchedUsers = users.Where(u => chatUsers.All(cu => cu.UserGuid != u.Guid))
                .Select(u => new {u.Guid, u.UserName})
                .ToArray();

            Clients.Caller.onSearchUsersForAddUsersToChatCaller(JsonConvert.SerializeObject(searchedUsers));
        }

        public void AddUsersForAddUsersToChat(string jsonUsers, string userGuid, string chatGuid)
        {
            var userElasticResult = _userRepository.Get(userGuid);
            if (!userElasticResult.Success)
            {
                Clients.Caller.onAddUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new { error = true, message = UserErrorMessage }));
                return;
            }
            var user = userElasticResult.Value;

            var usersGuids = JsonConvert.DeserializeObject<string[]>(jsonUsers);

            var usersElasticResult = _userRepository.GetAllByGuids(usersGuids);
            if (!usersElasticResult.Success)
            {
                Clients.Caller.onAddUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                return;
            }
            var users = usersElasticResult.Value;

            var chatUserElasticResult = _chatUserRepository.GetAllByChatGuid(chatGuid);
            if (!chatUserElasticResult.Success)
            {
                Clients.Caller.onAddUsersForAddUsersToChatCaller(
                    JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                return;
            }

            var chatUsers = chatUserElasticResult.Value;
            
            var newUsers = users.Where(u => chatUsers.All(cu => cu.UserGuid != u.Guid)).ToArray();

            foreach (var newUser in newUsers)
            {
                var elasticResult = _chatUserRepository.Add(chatGuid, newUser.Guid);
                if (!elasticResult.Success)
                {
                    Clients.Caller.onAddUsersForAddUsersToChatCaller(
                        JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                    return;
                }

                var messageElasticResult = _messageRepository.AddAdminMessage(chatGuid, user, newUser.UserName);
                if (!messageElasticResult.Success)
                {
                    Clients.Caller.onAddUsersForAddUsersToChatCaller(
                        JsonConvert.SerializeObject(new { error = true, message = ServerErrorMessage }));
                    return;
                }

                var message = JsonConvert.SerializeObject(messageElasticResult.Value);
                Clients.Group(chatGuid).onSendMessageOthers(message);

                foreach (var connectionId in newUser.ConnectionIds)
                    Groups.Add(connectionId, chatGuid);
            }
            
            var newUsersConnectionIds = newUsers.SelectMany(u => u.ConnectionIds).ToArray();
            Clients.Clients(newUsersConnectionIds).onAddUsersForAddUsersToChatNewUsers();
        }

        #endregion

        #region Private Methods

        private ElasticUser GetUserFromCookies(IDictionary<string, Cookie> cookies)
        {
            if (!cookies.ContainsKey(CookieName))
                return null;

            var elasticResult = _userRepository.CheckToken(cookies[CookieName].Value);
            return elasticResult.Success && elasticResult.Value != null ? elasticResult.Value : null;
        }

        #endregion
    }
}