var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
window.onload = function () {
    var client = Client.getInstance();
};

var sendData = function (data, client) {
    client.sendPackage(data);
};

var Client = (function () {
    function Client(websocket) {
        var _this = this;
        this._initialized = false;
        this._sessionId = null;
        this.sendPackage = function (data) {
            if (_this.ws.readyState == WebSocket.OPEN) {
                _this.ws.send(data);
            } else {
                alert('Socket not opened');
            }
        };
        this.sendMessage = function (message) {
            _this.sendPackage(JSON.stringify(message));
        };
        this.sendNotification = function (notification) {
            _this.sendPackage(JSON.stringify(notification));
        };
        this.processMessage = function (data) {
            alert(data);
        };
        this.startSession = function () {
            var session = Session.fromObject({ state: "new" });
            var jsonString = JSON.stringify(session);
            _this.sendPackage(jsonString);
            document.getElementById("btnIniciarSessao").disabled = true;
        };
        var instance = this;
        this.ws = websocket;
        this.ws.onmessage = function (event) {
            var data = JSON.parse(event.data);

            var envelope = Envelope.fromObject(data);

            if (envelope instanceof Envelope) {
                instance._sessionId = envelope.id;
            }

            var session = Session.fromObject(data);

            if (session instanceof Session && session.state) {
                if (session.state === "negotiating") {
                    instance._sessionId = session.id;
                    var sessionNegotiation = new Session();
                    sessionNegotiation.id = instance._sessionId;
                    sessionNegotiation.to = session.from;
                    sessionNegotiation.state = "negotiating";
                    sessionNegotiation.encryption = "none";
                    sessionNegotiation.compression = "none";
                    instance.sendPackage(sessionNegotiation);
                } else if (session.state === "authenticating") {
                    alert(event.data);
                }
            } else {
                alert(event.data);
            }
        };
        this.ws.onerror = function (event) {
            alert('error');
        };
        this.ws.onopen = function (event) {
            alert('opened');
            _this._initialized = true;
        };
        this.ws.onclose = function (event) {
            alert('closed');
        };
    }
    Client.getInstance = function () {
        if (Client._instance === null) {
            var socket = new WebSocket('ws://cake.takenet.com.br:55321/');
            Client._instance = new Client(socket);
        }
        return Client._instance;
    };

    Client.prototype.changeSocket = function (websocket) {
        Client._instance = new Client(websocket);
    };
    Client._instance = null;
    return Client;
})();

var Envelope = (function () {
    function Envelope(from, to, id) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        this.from = from;
        this.to = to;
        this.id = id;
    }
    Envelope.fromObject = function (object) {
        return new Envelope(object.from, object.to, object.id);
    };
    return Envelope;
})();

var Notification = (function (_super) {
    __extends(Notification, _super);
    function Notification() {
        _super.apply(this, arguments);
    }
    return Notification;
})(Envelope);

var Message = (function (_super) {
    __extends(Message, _super);
    function Message() {
        _super.apply(this, arguments);
    }
    return Message;
})(Envelope);

var Command = (function (_super) {
    __extends(Command, _super);
    function Command() {
        _super.apply(this, arguments);
    }
    return Command;
})(Envelope);

var Session = (function (_super) {
    __extends(Session, _super);
    function Session(from, to, id, state, mode, encryptionOptions, compressionOptions, compression, encryption) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        if (typeof state === "undefined") { state = null; }
        if (typeof mode === "undefined") { mode = null; }
        if (typeof encryptionOptions === "undefined") { encryptionOptions = null; }
        if (typeof compressionOptions === "undefined") { compressionOptions = null; }
        if (typeof compression === "undefined") { compression = null; }
        if (typeof encryption === "undefined") { encryption = null; }
        _super.call(this, from, to, id);
        this.from = from;
        this.to = to;
        this.id = id;
        this.state = state;
        this.mode = mode;
        this.encryptionOptions = encryptionOptions;
        this.compressionOptions = compressionOptions;
        this.compression = compression;
        this.encryption = encryption;
    }
    Session.fromObject = function (object) {
        return new Session(object.from, object.to, object.id, object.state, object.mode, object.encryptionOptions, object.compressionOptions, object.compression, object.encryption);
    };
    return Session;
})(Envelope);

var Reason = (function () {
    function Reason() {
    }
    return Reason;
})();

var Content = (function () {
    function Content() {
    }
    return Content;
})();
//# sourceMappingURL=app.js.map
