class Greeter {
    element: HTMLElement;
    span: HTMLElement;
    timerToken: number;

    constructor(element: HTMLElement) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }

    start() {
        this.timerToken = setInterval(() => this.span.innerHTML = new Date().toUTCString(), 500);
    }

    stop() {
        clearTimeout(this.timerToken);
    }

}

window.onload = () => {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
    var client = Client.getInstance();

};

var sendData = (data: string, client: Client) => {
    client.sendPackage(data);
}

class Client {

    private static _instance: Client = null;
    private _initialized: boolean = false;
    private _sessionId:string = null;
    ws: WebSocket;

    public static getInstance(): Client {
        if (Client._instance === null) {
            var socket = new WebSocket('ws://cake.takenet.com.br:55321/');
            Client._instance = new Client(socket);
        }
        return Client._instance;
    }

    public changeSocket(websocket: WebSocket) {
        Client._instance = new Client(websocket);
    }

    constructor(websocket: WebSocket) {
        this.ws = websocket;
        this.ws.onmessage = (event) => {

            var data = JSON.parse(event.data);

            var envelope = Envelope.fromObject(data);

            if (envelope instanceof Envelope) {
                this._sessionId = envelope.id;
            }

            var session = Session.fromObject(data);

            if (session instanceof Session && session.state) {
                if (session.state === "negotiating" ){
                    this._sessionId = session.id;
                    var sessionNegotiation = new Session();
                    sessionNegotiation.id = this._sessionId;
                    sessionNegotiation.to = session.from;
                    sessionNegotiation.state = "negotiating";
                    sessionNegotiation.encryption = "none";
                    sessionNegotiation.compression = "none";
                    this.sendPackage(sessionNegotiation);
                } else if (session.state === "authenticating") {
                    alert(event.data);
                }

            } else {
                alert(event.data);
            }
        }
        this.ws.onerror = (event) => {
            alert('error');
        }
        this.ws.onopen = (event) => {
            alert('opened');
            this._initialized = true;
        }
        this.ws.onclose = (event) => {
            alert('closed');
        }
    }

    sendPackage = (data: any) => {
        if (this.ws.readyState == WebSocket.OPEN) {
            this.ws.send(data);
        } else {
            alert('Socket not opened');
        }
    }

    sendMessage = (message: Message) => {
        this.sendPackage(JSON.stringify(message));
    }
    sendNotification = (notification: Notification) => {
        this.sendPackage(JSON.stringify(notification));
    }
    processMessage = (data: any) => {
        alert(data);
    }
    startSession = () => {
        var session = Session.fromObject({ state: "new" });
        var jsonString = JSON.stringify(session);
        this.sendPackage(jsonString);
        document.getElementById("btnIniciarSessao").disabled = true;
    }
    nego


}

class Envelope {

    constructor(public from: string = null, public to: string = null, public id: string = null) {
        
    }
    static fromObject(object: any) {
        return new Envelope(object.from, object.to, object.id);
    }
}

class Notification extends Envelope {
    event: string;
    reason: Reason
}

class Message extends Envelope {
    content: Content;
}

class Command extends Envelope {
    method: string;
    type: string;
    resource: any;
    status: string;
    reason: Reason;
}

class Session extends Envelope {

    constructor(
        public from: string = null,
        public to: string = null,
        public id: string = null,
        public state: string = null,
        public mode: string = null,
        public encryptionOptions: string[] = null,
        public compressionOptions: string[] = null,
        public compression: string = null,
        public encryption: string = null
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
            object.encryption);
    }
}

class Reason {
    code: number;
    description: string;
}

class Content {
    text: string;
}