module Lime.Protocol {

    import Model = Lime.Protocol.Model;

    export class LimeClient {

        private _initialized: boolean = false;
        private _sessionId: string = null;
        private _remoteNode: string = null;
        private _localNode: string = null;
        private _identity: string = null;
        private _password: string = null;
        private _sessionState: Model.SessionState = Model.SessionState.New;

        public receiveMessage: (message: Model.Message) => void;
        public receiveSession: (session: Model.Session) => void;
        public receiveCommand: (command: Model.Command) => void;
        public receiveNotification: (notification: Model.Notification) => void;
        public changedSessionState: (oldState: Model.SessionState, newState: Model.SessionState) => void;

        ws: WebSocket;

        constructor(private serverUrl: string) {
            this.ws = new WebSocket(serverUrl);

            var instance = this;
            this.receiveSession = this.receiveSessionInternal;

            this.ws.onmessage = (event) => {

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
                    if (this.receiveCommand) {
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
            }
        this.ws.onerror = (event) => {
                throw new Error(event.message);
            }
        this.ws.onopen = (event) => {
                this._sessionState = Model.SessionState.New;
                this._initialized = true;
            }
        this.ws.onclose = (event) => {

            }
    }

        sendMessage = (message: Model.Message) => {
            this.sendEnvelope(message);
        }

    sendNotification = (notification: Model.Notification) => {
            this.sendEnvelope(notification);
        }

    startSession = (identity: string, password: string) => {
            this._identity = identity;
            this._password = password;
            var session = Model.Session.fromObject({ state: Model.SessionState.New });
            this.sendEnvelope(session);
        }
        endSession = () => {
            if (this._sessionState != Model.SessionState.Established) {
                throw new Error("this session is not in a valid state to be terminated");
            }
            var session = new Model.Session(this._identity);
            session.state = Model.SessionState.Finishing;
            this.sendEnvelope(session);
        }

    private sendNegotiationgOption = (compressionOptions: string[], encryptionOptions: string[]) => {
            if (compressionOptions.indexOf('none') < 0) {
                throw new Error("Unsupported compression options");
            }
            if (encryptionOptions.indexOf('none') < 0) {
                throw new Error("Unsupported encryption options");
            }
            var sessionNegotiation = new Model.Session();
            sessionNegotiation.id = this._sessionId;
            sessionNegotiation.state = Model.SessionState.Negotiating;
            sessionNegotiation.encryption = compressionOptions[0];
            sessionNegotiation.compression = encryptionOptions[0];
            this.sendEnvelope(sessionNegotiation);
        }

    private sendAuthentication = (schemeOptions: string[]) => {
            if (schemeOptions.indexOf('plain') < 0) {
                throw new Error("Unsupported authentication scheme");
            }
            var session = Model.Session.fromObject(
                {
                    id: this._sessionId,
                    from: this._identity,
                    state: Model.SessionState.Authenticating,
                    scheme: 'plain',
                    authentication: new Model.PlainAuthentication(this._password)
                });
            this.sendEnvelope(session);

        }

    private sendEnvelope = (envelope: Model.Envelope) => {
            if (this.ws.readyState == WebSocket.OPEN) {
                var data = this.removeEmpty(envelope);
                this.ws.send(JSON.stringify(data));
            } else {
                alert('Socket not opened');
            }
        }

    private removeEmpty = target => {
            Object.keys(target).map(key => {
                if (target[key] instanceof Object) {
                    if (!Object.keys(target[key]).length && typeof target[key].getMonth !== 'function') {
                        delete target[key];
                    }
                    else {
                        this.removeEmpty(target[key]);
                    }
                }
                else if (target[key] === null) {
                    delete target[key];
                }
            });
            return target;
        };
    
    private receiveSessionInternal = (session: Model.Session) => {
            this._sessionId = session.id;
            if (this._sessionState != session.state) {
                var oldSessionState = this._sessionState;
                this._sessionState = session.state;
                this.changedSessionState(oldSessionState, this._sessionState);
            }

            switch (this._sessionState) {

                case Model.SessionState.Negotiating:
                    if (session.encryptionOptions || session.compressionOptions) {
                        this.sendNegotiationgOption(session.compressionOptions, session.encryptionOptions);
                    } else if (!session.encryption || !session.compression) {
                        this.ws.close(1, "invalid packageinvalid envelope was received");
                        throw new Error("invalid envelope was received on the client");
                    }
                    break;
                case Model.SessionState.Authenticating:
                    if (session.schemeOptions) {
                        this.sendAuthentication(session.schemeOptions);
                    }
                    break;
                case Model.SessionState.Established:
                    alert('Established: ' + JSON.stringify(session));
                    break;
                case Model.SessionState.Finished:
                    this.ws.close();
                    break;
                case Model.SessionState.Failed:
                    this.ws.close();
                    if (session.reason) {
                        throw new Error(session.reason.description);
                    } else {
                        throw new Error('The session has failed');
                    }
                default:
                    throw new Error('Invalid Session State');
            }
        }

}
}

