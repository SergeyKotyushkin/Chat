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

function createChat(model, server) {
    visibilityErrorDiv(false);
    visibilitySuccessDiv(false);

    var name = model.newChatName().trim();
    if (!name.length) {
        setErrorMessage(self, "Fill name of the new chat");
        return;
    }

    $.post("Chat/CreateChat", { name: name, guid: model.currentUser().Guid }, function (json) {
        var result = JSON.parse(json);

        if (result.error) {
            if (result.code === 1) {
                visibilityChatDiv(false);
            }

            setErrorMessage(model, result.message);
            return;
        }

        server.createChat(name, model.currentUser().Guid);
        getAllChats(model, model.currentUser());
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
    
    // create hub
    var chat = $.connection.chatHub;

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

        self.newChatName = ko.observable();
        self.createNewChat = function() {
            createChat(self, chat.server);
            newChatName(null);
        }
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

        self.afterRenderMessages = function(elements, data) {
            if (this.foreach()[this.foreach().length - 1] === data)
                $("#chat-messages").animate({ scrollTop: $("#chat-messages")[0].scrollHeight }, "slow");
        }
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

        // get all users
        spinAllUsersSpinner(true);
        spinAllChatsSpinner(true);
        setTimeout(function() {
            getAllUsers(viewModel, function() {
                spinAllUsersSpinner(false);
            });

            getAllChats(viewModel, function() {
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

        viewModel.messages.push(result);
    };

    visibilityLoadingDiv(true);
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