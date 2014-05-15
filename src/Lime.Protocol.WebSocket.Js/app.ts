
module Chat {

    import LM = Lime.Protocol.Model;
    export enum AvaliabilityStatus {
        Unavaliable,
        Connecting,
        Avaliable
    }

    export class ChatViewModel {


        constructor(private serverUrl: string) {
            this.client = new Lime.Protocol.LimeClient(this.serverUrl);
        }

        messages: KnockoutObservableArray<LM.Message> = ko.observableArray<LM.Message>();
        user: KnockoutObservable<string> = ko.observable<string>();
        server: KnockoutObservable<string> = ko.observable<string>();
        password: KnockoutObservable<string> = ko.observable<string>();
        textMessage: KnockoutObservable<string> = ko.observable<string>();
        messageDestination: KnockoutObservable<string> = ko.observable<string>();

        identity: KnockoutComputed<string> = ko.computed<string> (() =>{
            return this.user() + '@' + this.server();
        });
        avaliabilityStatus: KnockoutObservable<AvaliabilityStatus> = ko.observable<AvaliabilityStatus>(AvaliabilityStatus.Unavaliable);
        private client: Lime.Protocol.LimeClient;

        sendMessage = () => {
            var message = new LM.Message(this.identity());
            message.content = new LM.Content(this.textMessage());
            message.to = this.messageDestination();
            message.type = 'application/vnd.lime.text+json';
            this.messages.push(message);
            this.client.sendMessage(message);
        }

        login = () => {
            
            this.client.changedSessionState = this.stateChanged;
            this.client.receiveMessage = this.receiveMessage;
            this.client.receiveNotification = this.receiveNotification;
            this.client.receiveCommand = this.receiveCommand;
            this.client.startSession(this.identity(), this.password());
        }
        logout = () => {
            this.client.endSession();
        } 
        

         private stateChanged = (oldState: LM.SessionState, newState: LM.SessionState) => {
            switch (newState) {
                case LM.SessionState.Established:
                    this.avaliabilityStatus(AvaliabilityStatus.Avaliable);
                    break;
                case LM.SessionState.Authenticating:
                case LM.SessionState.Negotiating:
                    this.avaliabilityStatus(AvaliabilityStatus.Connecting);
                    break;
                case LM.SessionState.Failed:
                case LM.SessionState.Finished:
                case LM.SessionState.New:
                    this.avaliabilityStatus(AvaliabilityStatus.Unavaliable);
                    break;
                default:
                    throw new Error('Invalid session state');
            }
        }

        private receiveMessage = (message: LM.Message) => {
            this.messages.push(message);
        };

        private receiveNotification = (notification: LM.Notification) => {
            alert(JSON.stringify(notification.event));
        };

        private receiveCommand =  (command: LM.Command) => {
            alert(JSON.stringify(command.resource));
        };
    }



}