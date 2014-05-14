module Lime.Protocol.Model {

    export class Envelope {

        constructor(
            public from: string = null,
            public to: string = null,
            public id: string = null) {

        }
        static fromObject(object: any) {
            return new Envelope(object.from, object.to, object.id);
        }
    }

    export class SessionState {
        static New = 'new';
        static Negotiating = 'negotiating';
        static Authenticating = 'authenticating';
        static Established = 'established';
        static Finishing = 'finishing';
        static Finished = 'finished';
        static Failed = 'failed';
    }

    export class Notification extends Envelope {
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

    export class Message extends Envelope {
        constructor(
            public from: string = null,
            public to: string = null,
            public id: string = null,
            public content: Content = null,
            public type:string = null) {
            super(from, to, id);
        }

        static fromObject(object: any) {
            return new Message(
                object.from,
                object.to,
                object.id,
                object.Content,
                object.type
                );
        }
    }

    export class Command extends Envelope {

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

    export class Session extends Envelope {

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

    export class Authentication {

    }

    export class PlainAuthentication extends Authentication {
        password: string;
        constructor(password: string) {
            this.password = btoa(password);
            super();
        }
    }

    export class Reason {
        code: number;
        description: string;
    }

    export class Content {
        text: string;
    }
}