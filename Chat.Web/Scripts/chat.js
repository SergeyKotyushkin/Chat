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
            lastSendTime: (model.messages() != null && model.messages().length
                ? new Date(model.messages()[0].SendTime).getTime()
                : Date.now()),
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

        self.chats = ko.observableArray([]);
        self.users = ko.observableArray([]);
        self.messages = ko.observableArray([]);

        self.setChatClick = function (chat) {
            self.messages.removeAll();
            self.currentChat(chat);
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

        self.messageText = ko.observable(null);

        self.sendMessageClick = function () {
            var message = self.messageText().trim();
            if (message.length)
                chat.server.sendMessage(message, self.currentUser().Guid, self.currentChat().Guid);

            self.messageText(null);
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
            });
        });

        self.addUserViewModel = ko.observable(new addUserViewModel(self));
        self.createChatViewModel = ko.observable(new createChatViewModel(self));

        self.needMessagesScrollToBottom = ko.observable(false);
        self.messagesCount = 10;

        self.isMssagesLoading = ko.observable(false);
        self.allMessagesLoaded = ko.observable(false);
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
            });

            getAllChats(viewModel, function () {
                if (viewModel.chats().length === 1) {
                    viewModel.currentChat(viewModel.chats()[0]);
                }

                spinAllChatsSpinner(false);
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
        viewModel.updateChatsClick();
    };

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

        viewModel.userName("");
        viewModel.errorUserNameLengthMessageShow(false);
        viewModel.searchResults.removeAll();
        viewModel.selectedUsers.removeAll();
        $("#add-user-modal").modal("hide");
    }

    chat.client.onAddUsersForAddUsersToChatNewUsers = function () {
        viewModel.updateChatsClick();
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