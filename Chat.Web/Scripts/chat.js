$(document).ready(function () {
    
    function chatViewModel() {
        var self = this;

        self.currentChat = ko.observable(null);

        self.chats = ko.observableArray([]);

        self.users = ko.observableArray([]);
    }

    var viewModel = new chatViewModel();
    ko.applyBindings(viewModel);


    // create hub
    //var chat = $.connection.chatHub;

});