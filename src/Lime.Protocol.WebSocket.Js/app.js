var Chat;
(function (Chat) {
    var LM = Lime.Protocol.Model;
    (function (AvaliabilityStatus) {
        AvaliabilityStatus[AvaliabilityStatus["Unavaliable"] = 0] = "Unavaliable";
        AvaliabilityStatus[AvaliabilityStatus["Connecting"] = 1] = "Connecting";
        AvaliabilityStatus[AvaliabilityStatus["Avaliable"] = 2] = "Avaliable";
    })(Chat.AvaliabilityStatus || (Chat.AvaliabilityStatus = {}));
    var AvaliabilityStatus = Chat.AvaliabilityStatus;

    var ChatViewModel = (function () {
        function ChatViewModel(serverUrl) {
            var _this = this;
            this.serverUrl = serverUrl;
            this.messages = ko.observableArray();
            this.user = ko.observable();
            this.server = ko.observable();
            this.password = ko.observable();
            this.textMessage = ko.observable();
            this.messageDestination = ko.observable();
            this.identity = ko.computed(function () {
                return _this.user() + '@' + _this.server();
            });
            this.avaliabilityStatus = ko.observable(0 /* Unavaliable */);
            this.sendMessage = function () {
                var message = new LM.Message(_this.identity());
                message.content = new LM.Content(_this.textMessage());
                message.to = _this.messageDestination();
                message.type = 'application/vnd.lime.text+json';
                _this.messages.push(message);
                _this.client.sendMessage(message);
            };
            this.login = function () {
                _this.client.changedSessionState = _this.stateChanged;
                _this.client.receiveMessage = _this.receiveMessage;
                _this.client.receiveNotification = _this.receiveNotification;
                _this.client.receiveCommand = _this.receiveCommand;
                _this.client.startSession(_this.identity(), _this.password());
            };
            this.logout = function () {
                _this.client.endSession();
            };
            this.stateChanged = function (oldState, newState) {
                switch (newState) {
                    case LM.SessionState.Established:
                        _this.avaliabilityStatus(2 /* Avaliable */);
                        break;
                    case LM.SessionState.Authenticating:
                    case LM.SessionState.Negotiating:
                        _this.avaliabilityStatus(1 /* Connecting */);
                        break;
                    case LM.SessionState.Failed:
                    case LM.SessionState.Finished:
                    case LM.SessionState.New:
                        _this.avaliabilityStatus(0 /* Unavaliable */);
                        break;
                    default:
                        throw new Error('Invalid session state');
                }
            };
            this.receiveMessage = function (message) {
                _this.messages.push(message);
            };
            this.receiveNotification = function (notification) {
                alert(JSON.stringify(notification.event));
            };
            this.receiveCommand = function (command) {
                alert(JSON.stringify(command.resource));
            };
            this.client = new Lime.Protocol.LimeClient(this.serverUrl);
        }
        return ChatViewModel;
    })();
    Chat.ChatViewModel = ChatViewModel;
})(Chat || (Chat = {}));
//# sourceMappingURL=app.js.map
