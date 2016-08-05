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

function getAllUsers(model, user) {
    $.post("Chat/GetAllUsers", { guid: user.Guid }, function(json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            visibilityChatDiv(false);
            setErrorMessage(model, result.message);
            return;
        }

        model.users.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.users.push(result[i]);
        }
    });
}

function getAllChats(model, user) {
    $.post("Chat/GetAllChats", { guid: user.Guid }, function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            visibilityChatDiv(false);
            setErrorMessage(model, result.message);
            return;
        }

        model.chats.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.chats.push(result[i]);
        }
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

        self.newChatName = ko.observable();
        self.createNewChat = function () { createChat(self, chat.server); }
    }

    var viewModel = new chatViewModel();
    ko.applyBindings(viewModel);


    chat.client.onLoginCaller = function (json) {
        var result = JSON.parse(json);

        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        viewModel.currentUser(result.user);

        viewModel.users.removeAll();
        for (var i = 0; i < result.users.length; i++) {
            viewModel.users.push(result.users[i]);
        }
        // get all users
        //getAllUsers(viewModel, result);
    };
    
    chat.client.onLoginOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        //updateUsers(viewModel, result);
    };

    chat.client.onDisconnectOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        //updateUsers(viewModel, result);
    };

    $.connection.hub.start()
        .done(function() {
            chat.server.login();
        })
        .fail(function(e) {
            alert(e);
        });
});