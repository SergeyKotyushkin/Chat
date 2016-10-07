/* functions */
function visibilityErrorDiv(show) {
    if(show)
        $("#alert-error-div").removeClass("hidden");
    else
        $("#alert-error-div").addClass("hidden");
}

function visibilitySuccessDiv(show) {
    if (show)
        $("#alert-success-div").removeClass("hidden");
    else
        $("#alert-success-div").addClass("hidden");
}

function visibilityChatDiv(show) {
    if (show)
        $("#chat-div").removeClass("hidden");
    else
        $("#chat-div").addClass("hidden");
}

function visibilityLoadingDiv(show) {
    if (show)
        $("#loading-image").removeClass("hidden");
    else
        $("#loading-image").addClass("hidden");
}

function spinAllUsersSpinner(spin) {
    if (spin)
        $("#all-users-spinner").addClass("fa-spin");
    else
        $("#all-users-spinner").removeClass("fa-spin");
}

function spinAllChatsSpinner(spin) {
    if (spin)
        $("#all-chats-spinner").addClass("fa-spin");
    else
        $("#all-chats-spinner").removeClass("fa-spin");
}

function setErrorMessage(model, message) {
    model.errorMessage(message);
    model.hasErrorMessage(true);
    visibilityErrorDiv(true);
}

function setSuccessMessage(model, message) {
    model.successMessage(message);
    model.hasSuccessMessage(true);
    visibilitySuccessDiv(true);
}

function arrayFirstIndexOf(array, predicate, predicateOwner) {
    for (var i = 0, j = array.length; i < j; i++) {
        if (predicate.call(predicateOwner, array[i])) {
            return i;
        }
    }
    return -1;
}

function getAllUsers(model, callback) {
    var user = model.currentUser();
    $.post("Chat/GetAllUsers", { guid: user.Guid }, function(json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            visibilityChatDiv(false);
            setErrorMessage(model, result.message);
            callback();
            return;
        }

        model.users.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.users.push(result[i]);
        }

        callback();
    });
}

function getAllChats(model, callback) {
    var user = model.currentUser();
    $.post("Chat/GetAllChats", { guid: user.Guid }, function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            visibilityChatDiv(false);
            setErrorMessage(model, result.message);
            callback();
            return;
        }

        model.chats.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.chats.push(result[i]);
        }

        callback();
    });
}

function getMessages(model, callback) {
    var user = model.currentUser();
    model.isMssagesLoading(true);
    $.post("Chat/GetMessages",
        {
            userGuid: user.Guid,
            chatGuid: model.currentChat().Guid,
            lastSendDateString: (model.messages() != null && model.messages().length
                ? model.messages()[0].SendTime
                : ""),
            count: model.messagesCount
        },
        function(json) {
            var result = JSON.parse(json);

            if (result.error != null && result.error) {
                setErrorMessage(model, result.message);
                callback(model.messagesCount + 1);
                return;
            }

            for (var i = 0; i < result.length; i++) {
                model.messages.unshift(result[i]);
            }

            callback(result.length);
        })
        .always(function () {
            model.isMssagesLoading(false);
        }
    );
}

function getAllChatUsersForChat(model, chat) {
    $.post("Chat/GetAllChatUsersForChat", { chatGuid: chat.Guid }, function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(model, result.message);
            return;
        }

        model.chatUsers.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.chatUsers.push(result[i]);
        }
    });
}

function updateUsers(model, newUser) {
    var index = arrayFirstIndexOf(model.users(), function (user) { return user.Guid === newUser.Guid; });
    if (index !== -1) {
        model.users.replace(model.users()[index], newUser);
    } else {
        model.users.push(newUser);
    }
}

/* on document ready */
$(document).ready(function () {

    visibilityLoadingDiv(true);

    // create hub
    var chat = $.connection.chatHub;

    function addUserViewModel(parent) {
        var self = this;

        self.userName = ko.observable("");
        self.errorUserNameLengthMessageShow = ko.observable(false);
        
        self.searchResults = ko.observableArray([]);
        self.selectedUsers = ko.observableArray([]);

        self.searchClick = function () {
            self.errorUserNameLengthMessageShow(false);
            self.userName(self.userName().trim());

            if (self.userName().length < 2) {
                self.errorUserNameLengthMessageShow(true);
                return;
            }

            chat.server.searchUsersForAddUsersToChat(self.userName(), parent.currentUser().Guid, parent.currentChat().Guid);
        }

        self.selectUserClick = function(user) {
            if (!self.selectedUsers().includes(user.Guid)) {
                self.selectedUsers.push(user.Guid);
            } else {
                self.selectedUsers.remove(user.Guid);
            }
        }

        self.commitClick = function() {
            if (!self.selectedUsers().length)
                return;

            chat.server.addUsersForAddUsersToChat(JSON.stringify(self.selectedUsers()), parent.currentUser().Guid, parent.currentChat().Guid);
            
            self.userName("");
            self.errorUserNameLengthMessageShow(false);
            self.searchResults.removeAll();
            self.selectedUsers.removeAll();
            $("#add-user-modal").modal("hide");
        }
    }

    function createChatViewModel(parent) {
        var self = this;

        self.chatName = ko.observable("");
        self.errorChatNameLengthMessageShow = ko.observable(false);
        self.errorChatNameIncorrectCharactersMessageShow = ko.observable(false);

        self.commitClick = function () {
            self.errorChatNameLengthMessageShow(false);
            self.errorChatNameIncorrectCharactersMessageShow(false);
            self.chatName(self.chatName().trim());

            if (self.chatName().length < 2) {
                self.errorChatNameLengthMessageShow(true);
                return;
            }

            if (/[^a-zA-Z0-9 ]/g.test(self.chatName())) {
                self.errorChatNameIncorrectCharactersMessageShow(true);
                return;
            }

            chat.server.createChat(self.chatName(), parent.currentUser().Guid);

            self.chatName("");
            self.errorChatNameLengthMessageShow(false);
            self.errorChatNameIncorrectCharactersMessageShow(false);
            $("#create-chat-modal").modal("hide");
        }
    }

    function chatViewModel() {
        var self = this;

        self.currentChat = ko.observable(null);
        self.currentUser = ko.observable(null);
        
        self.hasErrorMessage = ko.observable(false);
        self.hasSuccessMessage = ko.observable(false);
        
        self.errorMessage = ko.observable(null);
        self.successMessage = ko.observable(null);

        self.messageText = ko.observable(null);

        self.selectedChatUser = ko.observable(null);

        self.needMessagesScrollToBottom = ko.observable(false);
        self.messagesCount = 10;
        self.isMssagesLoading = ko.observable(false);
        self.allMessagesLoaded = ko.observable(false);

        self.chats = ko.observableArray([]);
        self.users = ko.observableArray([]);
        self.messages = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatUserOutputs = ko.computed(function () {

            function findUser(user) {
                return user.Guid === this.Guid;
            }

            var chatRoomUsers = self.users().filter(function (user) {
                return  self.chatUsers().find(findUser, user);
            });

            return chatRoomUsers.sort(function (user1, user2) {
                if (user1.IsOnline === user2.IsOnline)
                    return user1.UserName.localeCompare(user2.UserName);
                else return user1.IsOnline < user2.IsOnline;
            });
        });

        self.addUserViewModel = ko.observable(new addUserViewModel(self));
        self.createChatViewModel = ko.observable(new createChatViewModel(self));

        self.closeMessageClick = function (item, event) {
            var alertContainer = $(event.target).closest(".alert-div");
            $(alertContainer).toggle();
        }

        self.setChatClick = function (currentChat) {
            self.messages.removeAll();
            self.currentChat(currentChat);
        }

        self.closeChatClick = function () {
            self.messages.removeAll();
            self.currentChat(null);
        }

        self.updateUsersClick = function () {
            spinAllUsersSpinner(true);

            getAllUsers(self, function() {
                spinAllUsersSpinner(false);
            });
        }

        self.updateChatsClick = function () {
            spinAllChatsSpinner(true);

            getAllChats(self, function () {
                spinAllChatsSpinner(false);
            });
        }

        self.sendMessageClick = function () {
            var message = self.messageText().trim();
            if (message.length)
                chat.server.sendMessage(message, self.currentUser().Guid, self.currentChat().Guid);

            self.messageText(null);
        }

        self.onEscapeChatModalOpenClick = function () {
            if (!self.currentChat())
                return;

            self.chatUsers.removeAll();
            getAllChatUsersForChat(self, self.currentChat());
        }

        self.escapeChatYesClick = function() {
            if (!self.currentChat() || (!self.selectedChatUser() && self.chatUsers().length !== 1 && self.currentChat().AdminGuid === self.currentUser().Guid)) {
                self.selectedChatUser(null);
                self.chatUsers.removeAll();
                $("#escape-chat-modal").modal("hide");
                return;
            }

            var mode = 2;
            if (self.selectedChatUser()) {
                mode = 0;
            } else if (self.chatUsers().length === 1) {
                mode = 1;
            }

            chat.server.escapeChat(self.currentChat().Guid, self.currentUser().Guid, mode, self.selectedChatUser() ? self.selectedChatUser().Guid : "");

            self.selectedChatUser(null);
            self.chatUsers.removeAll();
            $("#escape-chat-modal").modal("hide");
        }

        self.escapeChatNoClick = function() {
            self.selectedChatUser(null);
            self.chatUsers.removeAll();
            $("#escape-chat-modal").modal("hide");
        }

        self.newMessageTextKeyUp = function (d, e) {
            // send message on enter key
            if (e.keyCode === 13) {
                self.sendMessageClick();
            }
        }

        self.afterRenderMessages = function (elements, data) {
            if (this.foreach()[this.foreach().length - 1] === data && self.isMssagesLoading() === false) {
                self.isMssagesLoading(true);
                $("#chat-messages").animate({ scrollTop: $("#chat-messages")[0].scrollHeight }, {
                    duration: 500,
                    complete: function() {
                        self.isMssagesLoading(false);
                        self.needMessagesScrollToBottom(true);
                    }
                });
            }
        }

        self.onChatMessagesScroll = function(data, event) {
            var elem = event.target;
            var prevScrollHeight = $(elem)[0].scrollHeight;
            if (self.allMessagesLoaded() === false && self.isMssagesLoading() === false && $(elem)[0].scrollTop === 0) {
                getMessages(self, function(loadedCount) {
                    if (loadedCount < self.messagesCount)
                        self.allMessagesLoaded(true);

                    var newScrollHeight = $(elem)[0].scrollHeight;
                    $(elem).scrollTop(newScrollHeight - prevScrollHeight);
                    $(elem).animate({ scrollTop: newScrollHeight - prevScrollHeight - $(elem).height() / 2 }, 600);
                });
            }
        }

        self.currentChat.subscribe(function(newValue) {
            if (newValue == null)
                return;

            getMessages(self, function () {
                $("#chat-messages").animate({ scrollTop: $("#chat-messages")[0].scrollHeight }, "slow");
                self.needMessagesScrollToBottom(false);

                getAllChatUsersForChat(self, newValue);
            });
        });
    }

    var viewModel = new chatViewModel();
    ko.applyBindings(viewModel);


    chat.client.onLoginCaller = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }
        
        viewModel.currentUser(result);
        viewModel.needMessagesScrollToBottom(true);

        // get all users
        spinAllUsersSpinner(true);
        spinAllChatsSpinner(true);
        setTimeout(function() {
            getAllUsers(viewModel, function() {
                spinAllUsersSpinner(false);

                getAllChats(viewModel, function() {
                    if (viewModel.chats().length === 1) {
                        viewModel.currentChat(viewModel.chats()[0]);
                    }

                    spinAllChatsSpinner(false);
                });
            });
        }, 1000);
    };
    
    chat.client.onLoginOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        updateUsers(viewModel, result);
    };

    chat.client.onDisconnectOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        updateUsers(viewModel, result);
    };

    chat.client.onCreateChatCaller = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        setSuccessMessage(viewModel, "New chat was created successfuly");
        viewModel.chats.push(result);
        viewModel.chats.sort(function (chat1, chat2) {
            return chat1.Name.localeCompare(chat2.Name);
        });
    };

    chat.client.onEscapeChatCaller = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        viewModel.chats.remove(function(currentChat) { return currentChat.Guid === result.Guid });
        viewModel.currentChat(null);
    }

    chat.client.onEscapeChatOthers = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        var chatForUpdate = ko.utils.arrayFirst(viewModel.chats(), function (currentChat) { return currentChat.Guid === result.Guid });
        viewModel.chats.replace(chatForUpdate, result);
        viewModel.currentChat(result);
    }

    chat.client.onSendMessageCaller = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }
    };

    chat.client.onSendMessageOthers = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        if (result.chatGuid === viewModel.currentChat().Guid) {
            viewModel.needMessagesScrollToBottom(true);
            viewModel.messages.push(result.message);
        }
    };

    chat.client.onSearchUsersForAddUsersToChatCaller = function(json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        viewModel.addUserViewModel().searchResults.removeAll();
        result.forEach(function(searchResult) {
            viewModel.addUserViewModel().searchResults.push(searchResult);
        });
    }

    chat.client.onAddUsersForAddUsersToChatCaller = function(json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }
    }

    chat.client.onAddUsersForAddUsersToChatNewUsers = function (json) {
        var result = JSON.parse(json);

        viewModel.chats.push(result);
    }

    setTimeout(function() {
        $.connection.hub.start()
            .done(function() {
                chat.server.login();
                visibilityLoadingDiv(false);
            })
            .fail(function(e) {
                alert(e);
                visibilityLoadingDiv(false);
            });
    }, 1000); // Wait 1 second.
});