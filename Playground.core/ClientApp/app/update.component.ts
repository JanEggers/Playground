import {
    Component,
    Injectable,
    Http,
    Headers,
    HubConnection,
    HttpConnection,
    TransportType,
} from "./vendor";

//npm install signalr-client --registry https://dotnet.myget.org/f/aspnetcore-ci-dev/npm/


@Component({
    selector: "update",
    template: `
<h2>Update</h2>

<ul>
    <li *ngFor='let message of messages' >{{message}}<li>
</ul>

<input type="text" [(ngModel)]="message" >
<button (click)="send()">send!</button>
<p *ngIf="error" >{{error}}</p>
`
})
export class UpdateComponent {
    connection: HubConnection;

    messages: string[];

    message: string;

    error;

    start: Promise<void>;

    constructor() { 
        this.messages = [];

        this.connection = new HubConnection(`http://${document.location.host}/updates`, { transport: TransportType.WebSockets });

        this.connection.on('Send', (message) => {
            this.messages.push(message);
        });

        this.start = this.connection.start();
    }

    async send() {
        try {
            await this.start;
            this.error = "";
            await this.connection.invoke('Send', this.message);
            this.message = "";
        } catch (error) {
            this.error = error;
        }
    }
}