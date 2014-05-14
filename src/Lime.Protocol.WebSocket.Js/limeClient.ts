window.onload = () => {
    var client = LimeClient.getInstance();

};

class LimeClient {

    private static _instance: LimeClient = null;
    private _initialized: boolean = false;
    private _sessionId: string = null;
    private _remoteNode: string = null;
    private _localNode: string = null;
    private _identity: string = null;
    private _password: string = null;
    private _sessionState: SessionState = SessionState.New;

    public receiveMessage: (message: Message) => void;
    public receiveSession: (session: Session) => void;
    public receiveCommand: (command: Command) => void;
    public receiveNotification: (notification: Notification) => void;

    ws: WebSocket;

    public static getInstance(): LimeClient {
        if (LimeClient._instance === null) {
            var socket = new WebSocket('ws://cake.takenet.com.br:55321/');
            LimeClient._instance = new LimeClient(socket);
        }
        return LimeClient._instance;
    }

    public changeSocket(websocket: WebSocket) {
        LimeClient._instance = new LimeClient(websocket);
    }

    constructor(websocket: WebSocket) {

        var instance = this;
        this.ws = websocket;
        this.receiveSession = this.receiveSessionInternal;

        this.ws.onmessage = (event) => {

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
                if (this.receiveCommand) {
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
        }
        this.ws.onerror = (event) => {
            throw new Error(event.message);
        }
        this.ws.onopen = (event) => {
            this._sessionState = SessionState.New;
            this._initialized = true;
        }
        this.ws.onclose = (event) => {
            
        }
    }

    receiveSessionInternal = (session: Session) => {
        this._sessionId = session.id;
        this._sessionState = session.state;
        switch (this._sessionState) {

            case SessionState.Negotiating:
                if (session.encryptionOptions || session.compressionOptions) {
                    this.sendNegotiationgOption(session.compressionOptions, session.encryptionOptions);
                } else if (!session.encryption || !session.compression) {
                    this.ws.close(1, "invalid packageinvalid envelope was received");
                    throw new Error("invalid envelope was received on the client");
                }
                break;
            case SessionState.Authenticating:
                if (session.schemeOptions) {
                    this.sendAuthentication(session.schemeOptions);
                }
                break;
            case SessionState.Established:
                alert('Established: ' + JSON.stringify(session));
                break;
            case SessionState.Finished:
                this.ws.close();
                break;
            case SessionState.Failed:
                this.ws.close();
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
            this._sessionId = session.id;

        } else if (session.state === SessionState.Authenticating) {
            alert(event.data);
        }
    }

    sendEnvelope = (envelope: Envelope) => {
        if (this.ws.readyState == WebSocket.OPEN) {
            var data = JSON.stringify(envelope);
            this.ws.send(data);
        } else {
            alert('Socket not opened');
        }
    }

    sendMessage = (message: Message) => {
        this.sendEnvelope(message);
    }
    sendNotification = (notification: Notification) => {
        this.sendEnvelope(notification);
    }

    startSession = (identity: string, password: string) => {
        this._identity = identity;
        this._password = password;
        var session = Session.fromObject({ state: SessionState.New });
        this.sendEnvelope(session);
    }

    sendNegotiationgOption = (compressionOptions: string[], encryptionOptions: string[]) => {
        if (compressionOptions.indexOf('none') < 0) {
            throw new Error("Unsupported compression options");
        }
        if (encryptionOptions.indexOf('none') < 0) {
            throw new Error("Unsupported encryption options");
        }
        var sessionNegotiation = new Session();
        sessionNegotiation.id = this._sessionId;
        sessionNegotiation.state = SessionState.Negotiating;
        sessionNegotiation.encryption = compressionOptions[0];
        sessionNegotiation.compression = encryptionOptions[0];
        this.sendEnvelope(sessionNegotiation);
    }

    sendAuthentication = (schemeOptions: string[]) => {
        if (schemeOptions.indexOf('plain') < 0) {
            throw new Error("Unsupported authentication scheme");
        }
        var session = Session.fromObject(
            {
                id: this._sessionId,
                from: this._identity,
                state: SessionState.Authenticating,
                scheme: 'plain',
                authentication: new PlainAuthentication(this._password)
            });
        this.sendEnvelope(session);

    }

}


class Envelope {

    constructor(public from: string = null, public to: string = null, public id: string = null) {

    }
    static fromObject(object: any) {
        return new Envelope(object.from, object.to, object.id);
    }
}

class SessionState {
    static New = 'new';
    static Negotiating = 'negotiating';
    static Authenticating = 'authenticating';
    static Established = 'established';
    static Finished = 'finished';
    static Failed = 'failed';
}

class Notification extends Envelope {
    constructor(
        public from: string = null,
        public to: string = null,
        public id: string = null,
        public event: string = null,
        public reason: Reason = null) {
        super(from, to, id);
    }

    static fromObject(object: any) {
        return new Notification(
            object.from,
            object.to,
            object.id,
            object.event,
            object.reason
            );
    }
}

class Message extends Envelope {
    constructor(
        public from: string = null,
        public to: string = null,
        public id: string = null,
        public content: Content = null) {
        super(from, to, id);
    }

    static fromObject(object: any) {
        return new Message(
            object.from,
            object.to,
            object.id,
            object.Content
            );
    }
}

class Command extends Envelope {

    constructor(
        public from: string = null,
        public to: string = null,
        public id: string = null,
        public method: string = null,
        public type: string = null,
        public resource: any = null,
        public status: string = null,
        public reason: Reason = null) {
        super(from, to, id);
    }

    static fromObject(object: any) {
        return new Command(
            object.from,
            object.to,
            object.id,
            object.method,
            object.type,
            object.resource,
            object.status,
            object.reason
        );
    }
}

class Session extends Envelope {

    constructor(
        public from: string = null,
        public to: string = null,
        public id: string = null,
        public state: SessionState = null,
        public mode: string = null,
        public encryptionOptions: string[]= null,
        public compressionOptions: string[]= null,
        public compression: string = null,
        public encryption: string = null,
        public schemeOptions: string[]= null,
        public scheme: string = null,
        public authentication: Authentication = null,
        public reason: Reason = null
        ) {
        super(from, to, id);
    }
    static fromObject(object: any) {
        return new Session(
            object.from,
            object.to,
            object.id,
            object.state,
            object.mode,
            object.encryptionOptions,
            object.compressionOptions,
            object.compression,
            object.encryption,
            object.schemeOptions,
            object.scheme,
            object.authentication,
            object.reason
            );
    }
}

class Authentication {

}
class PlainAuthentication extends Authentication {
    password: string;
    constructor(password: string) {
        this.password = btoa(password);
        super();
    }
}

class Reason {
    code: number;
    description: string;
}

class Content {
    text: string;
}