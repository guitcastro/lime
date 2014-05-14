var Lime;
(function (Lime) {
    (function (Protocol) {
        var Model = Lime.Protocol.Model;

        var LimeClient = (function () {
            function LimeClient(serverUrl) {
                var _this = this;
                this.serverUrl = serverUrl;
                this._initialized = false;
                this._sessionId = null;
                this._remoteNode = null;
                this._localNode = null;
                this._identity = null;
                this._password = null;
                this._sessionState = Model.SessionState.New;
                this.sendMessage = function (message) {
                    _this.sendEnvelope(message);
                };
                this.sendNotification = function (notification) {
                    _this.sendEnvelope(notification);
                };
                this.startSession = function (identity, password) {
                    _this._identity = identity;
                    _this._password = password;
                    var session = Model.Session.fromObject({ state: Model.SessionState.New });
                    _this.sendEnvelope(session);
                };
                this.endSession = function () {
                    if (_this._sessionState != Model.SessionState.Established) {
                        throw new Error("this session is not in a valid state to be terminated");
                    }
                    var session = new Model.Session(_this._identity);
                    session.state = Model.SessionState.Finishing;
                    _this.sendEnvelope(session);
                };
                this.sendNegotiationgOption = function (compressionOptions, encryptionOptions) {
                    if (compressionOptions.indexOf('none') < 0) {
                        throw new Error("Unsupported compression options");
                    }
                    if (encryptionOptions.indexOf('none') < 0) {
                        throw new Error("Unsupported encryption options");
                    }
                    var sessionNegotiation = new Model.Session();
                    sessionNegotiation.id = _this._sessionId;
                    sessionNegotiation.state = Model.SessionState.Negotiating;
                    sessionNegotiation.encryption = compressionOptions[0];
                    sessionNegotiation.compression = encryptionOptions[0];
                    _this.sendEnvelope(sessionNegotiation);
                };
                this.sendAuthentication = function (schemeOptions) {
                    if (schemeOptions.indexOf('plain') < 0) {
                        throw new Error("Unsupported authentication scheme");
                    }
                    var session = Model.Session.fromObject({
                        id: _this._sessionId,
                        from: _this._identity,
                        state: Model.SessionState.Authenticating,
                        scheme: 'plain',
                        authentication: new Model.PlainAuthentication(_this._password)
                    });
                    _this.sendEnvelope(session);
                };
                this.sendEnvelope = function (envelope) {
                    if (_this.ws.readyState == WebSocket.OPEN) {
                        var data = _this.removeEmpty(envelope);
                        _this.ws.send(JSON.stringify(data));
                    } else {
                        alert('Socket not opened');
                    }
                };
                this.removeEmpty = function (target) {
                    Object.keys(target).map(function (key) {
                        if (target[key] instanceof Object) {
                            if (!Object.keys(target[key]).length && typeof target[key].getMonth !== 'function') {
                                delete target[key];
                            } else {
                                _this.removeEmpty(target[key]);
                            }
                        } else if (target[key] === null) {
                            delete target[key];
                        }
                    });
                    return target;
                };
                this.receiveSessionInternal = function (session) {
                    _this._sessionId = session.id;
                    if (_this._sessionState != session.state) {
                        var oldSessionState = _this._sessionState;
                        _this._sessionState = session.state;
                        _this.changedSessionState(oldSessionState, _this._sessionState);
                    }

                    switch (_this._sessionState) {
                        case Model.SessionState.Negotiating:
                            if (session.encryptionOptions || session.compressionOptions) {
                                _this.sendNegotiationgOption(session.compressionOptions, session.encryptionOptions);
                            } else if (!session.encryption || !session.compression) {
                                _this.ws.close(1, "invalid packageinvalid envelope was received");
                                throw new Error("invalid envelope was received on the client");
                            }
                            break;
                        case Model.SessionState.Authenticating:
                            if (session.schemeOptions) {
                                _this.sendAuthentication(session.schemeOptions);
                            }
                            break;
                        case Model.SessionState.Established:
                            alert('Established: ' + JSON.stringify(session));
                            break;
                        case Model.SessionState.Finished:
                            _this.ws.close();
                            break;
                        case Model.SessionState.Failed:
                            _this.ws.close();
                            if (session.reason) {
                                throw new Error(session.reason.description);
                            } else {
                                throw new Error('The session has failed');
                            }
                        default:
                            throw new Error('Invalid Session State');
                    }
                };
                this.ws = new WebSocket(serverUrl);

                var instance = this;
                this.receiveSession = this.receiveSessionInternal;

                this.ws.onmessage = function (event) {
                    var data = JSON.parse(event.data);
                    if (data.event) {
                        if (instance.receiveNotification) {
                            var notification = Model.Notification.fromObject(data);
                            instance.receiveNotification(notification);
                        }
                    } else if (data.content) {
                        if (instance.receiveMessage) {
                            var message = Model.Message.fromObject(data);
                            instance.receiveMessage(message);
                        }
                    } else if (data.method) {
                        if (_this.receiveCommand) {
                            var command = Model.Command.fromObject(data);
                            instance.receiveCommand(command);
                        }
                    } else if (data.state) {
                        var sesssion = Model.Session.fromObject(data);
                        if (instance.receiveCommand) {
                            instance.receiveSession(sesssion);
                        }
                    } else {
                        throw new Error("Unknown envelope type");
                    }
                };
                this.ws.onerror = function (event) {
                    throw new Error(event.message);
                };
                this.ws.onopen = function (event) {
                    _this._sessionState = Model.SessionState.New;
                    _this._initialized = true;
                };
                this.ws.onclose = function (event) {
                };
            }
            return LimeClient;
        })();
        Protocol.LimeClient = LimeClient;
    })(Lime.Protocol || (Lime.Protocol = {}));
    var Protocol = Lime.Protocol;
})(Lime || (Lime = {}));
//# sourceMappingURL=Lime.Protocol.Client.js.map
