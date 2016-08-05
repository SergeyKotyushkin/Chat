/* functions */
function setErrorMessage(model, message) {
    model.errorMessage(message);
    model.hasErrorMessage(true);
    $("#alert-error-div").removeClass("hidden");
}

function setSuccessMessage(model, message) {
    model.successMessage(message);
    model.hasSuccessMessage(true);
    $("#alert-success-div").removeClass("hidden");
}

function getAllUsers(model, user) {
    $.post("Chat/GetAllUsers", { guid: user.Guid }, function(json) {
        var result = JSON.parse(json);
        model.users.removeAll();
        for (var i = 0; i < result.length; i++) {
            model.users.push(result[i]);
        }
    });
}

/* on document ready */
$(document).ready(function () {
    
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
    }

    var viewModel = new chatViewModel();
    ko.applyBindings(viewModel);


    // create hub
    var chat = $.connection.chatHub;

    chat.client.onLoginCaller = function (json) {
        var result = JSON.parse(json);
        if (result.error != null && result.error) {
            setErrorMessage(viewModel, result.message);
            return;
        }

        $("#ialert-success-div").removeClass("hidden");
        viewModel.currentUser(result);
        setSuccessMessage(viewModel, "you are " + result.UserName);

        // get all users
        getAllUsers(viewModel, result);
    };
    
    chat.client.onLoginOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        setSuccessMessage(viewModel, result.UserName + " connected");
    };

    chat.client.onDisconnectOthers = function (json) {
        if (viewModel.currentUser() == null)
            return;

        var result = JSON.parse(json);
        setSuccessMessage(viewModel, result.UserName + " disconnected");
    };

    $.connection.hub.start()
        .done(function() {
            chat.server.login();
        })
        .fail(function(e) {
            alert(e);
        });
});