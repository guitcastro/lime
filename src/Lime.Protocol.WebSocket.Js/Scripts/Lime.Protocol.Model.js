var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var Lime;
(function (Lime) {
    (function (Protocol) {
        (function (Model) {
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
            Model.Envelope = Envelope;

            var SessionState = (function () {
                function SessionState() {
                }
                SessionState.New = 'new';
                SessionState.Negotiating = 'negotiating';
                SessionState.Authenticating = 'authenticating';
                SessionState.Established = 'established';
                SessionState.Finishing = 'finishing';
                SessionState.Finished = 'finished';
                SessionState.Failed = 'failed';
                return SessionState;
            })();
            Model.SessionState = SessionState;

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
                    var reason = Reason.fromObject(object.reason);
                    return new Notification(object.from, object.to, object.id, object.event, reason);
                };
                return Notification;
            })(Envelope);
            Model.Notification = Notification;

            var Message = (function (_super) {
                __extends(Message, _super);
                function Message(from, to, id, content, type) {
                    if (typeof from === "undefined") { from = null; }
                    if (typeof to === "undefined") { to = null; }
                    if (typeof id === "undefined") { id = null; }
                    if (typeof content === "undefined") { content = null; }
                    if (typeof type === "undefined") { type = null; }
                    _super.call(this, from, to, id);
                    this.from = from;
                    this.to = to;
                    this.id = id;
                    this.content = content;
                    this.type = type;
                }
                Message.fromObject = function (object) {
                    if (object) {
                        var content = Content.fromObject(object.content);
                        return new Message(object.from, object.to, object.id, content, object.type);
                    }
                    return null;
                };
                return Message;
            })(Envelope);
            Model.Message = Message;

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
                    if (object) {
                        var reason = Reason.fromObject(object.reason);
                        return new Command(object.from, object.to, object.id, object.method, object.type, object.resource, object.status, reason);
                    }
                    return null;
                };
                return Command;
            })(Envelope);
            Model.Command = Command;

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
                    if (object) {
                        var authentication = Authentication.fromObject(object.authentication);
                        var reason = Reason.fromObject(object.reason);
                        return new Session(object.from, object.to, object.id, object.state, object.mode, object.encryptionOptions, object.compressionOptions, object.compression, object.encryption, object.schemeOptions, object.scheme, authentication, reason);
                    }
                    return null;
                };
                return Session;
            })(Envelope);
            Model.Session = Session;

            var Authentication = (function () {
                function Authentication() {
                }
                Authentication.fromObject = function (object) {
                    if (object) {
                        return new Authentication();
                    }
                    return null;
                };
                return Authentication;
            })();
            Model.Authentication = Authentication;

            var PlainAuthentication = (function (_super) {
                __extends(PlainAuthentication, _super);
                function PlainAuthentication(password) {
                    this.password = btoa(password);
                    _super.call(this);
                }
                return PlainAuthentication;
            })(Authentication);
            Model.PlainAuthentication = PlainAuthentication;

            var Reason = (function () {
                function Reason(code, description) {
                    this.code = code;
                    this.description = description;
                }
                Reason.fromObject = function (object) {
                    if (object) {
                        return new Reason(object.code, object.description);
                    }
                    return null;
                };
                return Reason;
            })();
            Model.Reason = Reason;

            var Content = (function () {
                function Content(text) {
                    this.text = text;
                }
                Content.fromObject = function (object) {
                    if (object) {
                        return new Content(object.text);
                    }
                    return null;
                };
                return Content;
            })();
            Model.Content = Content;
        })(Protocol.Model || (Protocol.Model = {}));
        var Model = Protocol.Model;
    })(Lime.Protocol || (Lime.Protocol = {}));
    var Protocol = Lime.Protocol;
})(Lime || (Lime = {}));
//# sourceMappingURL=Lime.Protocol.Model.js.map
