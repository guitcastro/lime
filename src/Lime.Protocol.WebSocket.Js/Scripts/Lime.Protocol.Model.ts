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

        static fromObject(object: any): Notification {

            var reason = Reason.fromObject(object.reason);
            return new Notification(
                object.from,
                object.to,
                object.id,
                object.event,
                reason
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

        static fromObject(object: any): Message{
            if (object) {
                var content: Content = Content.fromObject(object.content);
                return new Message(
                    object.from,
                    object.to,
                    object.id,
                    content,
                    object.type
                    );
            }
            return null;
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

        static fromObject(object: any): Command {
            if (object) {
                var reason = Reason.fromObject(object.reason);
                return new Command(
                    object.from,
                    object.to,
                    object.id,
                    object.method,
                    object.type,
                    object.resource,
                    object.status,
                    reason
                    );
            }
            return null;
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
        static fromObject(object: any) : Session {
            if (object) {
                var authentication = Authentication.fromObject(object.authentication);
                var reason = Reason.fromObject(object.reason);
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
                    authentication,
                    reason
                    );
            }
            return null;
        }
    }

    export class Authentication {
        static fromObject(object: any): Authentication {
            if (object) {
                return new Authentication();
            }
            return null;
        }
    }

    export class PlainAuthentication extends Authentication {
        password: string;
        constructor(password: string) {
            this.password = btoa(password);
            super();
        }
    }

    export class Reason {

        constructor(public code: number, public description: string) {
            
        }
        
        static fromObject(object: any) : Reason {
            if (object) {
                return new Reason(object.code, object.description);
            }
            return null;
        }
    }

    export class Content {
        constructor(public text: string) {
            
        }
        static fromObject(object: any): Content {
            if (object) {
                return new Content(object.text);
            }
            return null;
        }
    }
}