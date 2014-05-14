var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
window.onload = function () {
    var client = Client.getInstance();
};

var Client = (function () {
    function Client(websocket) {
        var _this = this;
        this._initialized = false;
        this._sessionId = null;
        this._remoteNode = null;
        this._localNode = null;
        this._identity = null;
        this._password = null;
        this._sessionState = SessionState.New;
        this.receiveSessionInternal = function (session) {
            _this._sessionId = session.id;
            _this._sessionState = session.state;
            switch (_this._sessionState) {
                case SessionState.Negotiating:
                    if (session.encryptionOptions || session.compressionOptions) {
                        _this.sendNegotiationgOption(session.compressionOptions, session.encryptionOptions);
                    } else if (!session.encryption || !session.compression) {
                        _this.ws.close(1, "invalid packageinvalid envelope was received");
                        throw new Error("invalid envelope was received on the client");
                    }
                    break;
                case SessionState.Authenticating:
                    if (session.schemeOptions) {
                        _this.sendAuthentication(session.schemeOptions);
                    }
                    break;
                case SessionState.Established:
                    alert('Established: ' + JSON.stringify(session));
                    break;
                case SessionState.Finished:
                    _this.ws.close();
                    break;
                case SessionState.Failed:
                    _this.ws.close();
                    if (session.reason) {
                        throw new Error(session.reason.description);
                    } else {
                        throw new Error('The session has failed');
                    }
                    break;

                default:
                    throw new Error('Invalid Session State');
            }

            if (session.state === SessionState.Negotiating) {
                _this._sessionId = session.id;
            } else if (session.state === SessionState.Authenticating) {
                alert(event.data);
            }
        };
        this.sendEnvelope = function (envelope) {
            if (_this.ws.readyState == WebSocket.OPEN) {
                var data = JSON.stringify(envelope);
                _this.ws.send(data);
            } else {
                alert('Socket not opened');
            }
        };
        this.sendMessage = function (message) {
            _this.sendEnvelope(message);
        };
        this.sendNotification = function (notification) {
            _this.sendEnvelope(notification);
        };
        this.startSession = function (identity, password) {
            _this._identity = identity;
            _this._password = password;
            var session = Session.fromObject({ state: SessionState.New });
            _this.sendEnvelope(session);
            document.getElementById("btnIniciarSessao").disabled = true;
        };
        this.sendNegotiationgOption = function (compressionOptions, encryptionOptions) {
            if (compressionOptions.indexOf('none') < 0) {
                throw new Error("Unsupported compression options");
            }
            if (encryptionOptions.indexOf('none') < 0) {
                throw new Error("Unsupported encryption options");
            }

            var sessionNegotiation = new Session();
            sessionNegotiation.id = _this._sessionId;
            sessionNegotiation.state = SessionState.Negotiating;
            sessionNegotiation.encryption = compressionOptions[0];
            sessionNegotiation.compression = encryptionOptions[0];
            _this.sendEnvelope(sessionNegotiation);
        };
        this.sendAuthentication = function (schemeOptions) {
            if (schemeOptions.indexOf('plain') < 0) {
                throw new Error("Unsupported authentication scheme");
            }
            var session = Session.fromObject({
                id: _this._sessionId,
                from: _this._identity,
                state: SessionState.Authenticating,
                scheme: 'plain',
                authentication: new PlainAuthentication(_this._password)
            });
            _this.sendEnvelope(session);
        };
        var instance = this;
        this.ws = websocket;
        this.receiveSession = this.receiveSessionInternal;

        this.ws.onmessage = function (event) {
            document.getElementById('lastMessageReceivedContent').innerHTML = event.data;

            var data = JSON.parse(event.data);
            if (data.event) {
                if (instance.receiveNotification) {
                    var notification = Notification.fromObject(data);
                    instance.receiveNotification(notification);
                }
            } else if (data.content) {
                if (instance.receiveMessage) {
                    var message = Message.fromObject(data);
                    instance.receiveMessage(message);
                }
            } else if (data.method) {
                if (_this.receiveCommand) {
                    var command = Command.fromObject(data);
                    instance.receiveCommand(command);
                }
            } else if (data.state) {
                var sesssion = Session.fromObject(data);
                if (instance.receiveCommand) {
                    instance.receiveSession(sesssion);
                }
            } else {
                throw new Error("Unknown envelope type");
            }
        };
        this.ws.onerror = function (event) {
            alert('error');
        };
        this.ws.onopen = function (event) {
            _this._sessionState = SessionState.New;
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

var SessionState = (function () {
    function SessionState() {
    }
    SessionState.New = 'new';
    SessionState.Negotiating = 'negotiating';
    SessionState.Authenticating = 'authenticating';
    SessionState.Established = 'established';
    SessionState.Finished = 'finished';
    SessionState.Failed = 'failed';
    return SessionState;
})();

var Notification = (function (_super) {
    __extends(Notification, _super);
    function Notification(from, to, id, event, reason) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        if (typeof event === "undefined") { event = null; }
        if (typeof reason === "undefined") { reason = null; }
        _super.call(this, from, to, id);
        this.from = from;
        this.to = to;
        this.id = id;
        this.event = event;
        this.reason = reason;
    }
    Notification.fromObject = function (object) {
        return new Notification(object.from, object.to, object.id, object.event, object.reason);
    };
    return Notification;
})(Envelope);

var Message = (function (_super) {
    __extends(Message, _super);
    function Message(from, to, id, content) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        if (typeof content === "undefined") { content = null; }
        _super.call(this, from, to, id);
        this.from = from;
        this.to = to;
        this.id = id;
        this.content = content;
    }
    Message.fromObject = function (object) {
        return new Message(object.from, object.to, object.id, object.Content);
    };
    return Message;
})(Envelope);

var Command = (function (_super) {
    __extends(Command, _super);
    function Command(from, to, id, method, type, resource, status, reason) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        if (typeof method === "undefined") { method = null; }
        if (typeof type === "undefined") { type = null; }
        if (typeof resource === "undefined") { resource = null; }
        if (typeof status === "undefined") { status = null; }
        if (typeof reason === "undefined") { reason = null; }
        _super.call(this, from, to, id);
        this.from = from;
        this.to = to;
        this.id = id;
        this.method = method;
        this.type = type;
        this.resource = resource;
        this.status = status;
        this.reason = reason;
    }
    Command.fromObject = function (object) {
        return new Command(object.from, object.to, object.id, object.method, object.type, object.resource, object.status, object.reason);
    };
    return Command;
})(Envelope);

var Session = (function (_super) {
    __extends(Session, _super);
    function Session(from, to, id, state, mode, encryptionOptions, compressionOptions, compression, encryption, schemeOptions, scheme, authentication, reason) {
        if (typeof from === "undefined") { from = null; }
        if (typeof to === "undefined") { to = null; }
        if (typeof id === "undefined") { id = null; }
        if (typeof state === "undefined") { state = null; }
        if (typeof mode === "undefined") { mode = null; }
        if (typeof encryptionOptions === "undefined") { encryptionOptions = null; }
        if (typeof compressionOptions === "undefined") { compressionOptions = null; }
        if (typeof compression === "undefined") { compression = null; }
        if (typeof encryption === "undefined") { encryption = null; }
        if (typeof schemeOptions === "undefined") { schemeOptions = null; }
        if (typeof scheme === "undefined") { scheme = null; }
        if (typeof authentication === "undefined") { authentication = null; }
        if (typeof reason === "undefined") { reason = null; }
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
        this.schemeOptions = schemeOptions;
        this.scheme = scheme;
        this.authentication = authentication;
        this.reason = reason;
    }
    Session.fromObject = function (object) {
        return new Session(object.from, object.to, object.id, object.state, object.mode, object.encryptionOptions, object.compressionOptions, object.compression, object.encryption, object.schemeOptions, object.scheme, object.authentication, object.reason);
    };
    return Session;
})(Envelope);

var Authentication = (function () {
    function Authentication() {
    }
    return Authentication;
})();
var PlainAuthentication = (function (_super) {
    __extends(PlainAuthentication, _super);
    function PlainAuthentication(password) {
        this.password = btoa(password);
        _super.call(this);
    }
    return PlainAuthentication;
})(Authentication);

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
