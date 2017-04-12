import {
    Component,
    Injectable,
    Http,
    Headers,
} from "./vendor";

//npm install signalr-client --registry https://dotnet.myget.org/f/aspnetcore-ci-dev/npm/
import {
    HubConnection
} from 'signalr-client';

@Component({
    selector: "update",
    template: `
<h2>Update</h2>

<ul>
    <li *ngFor='let message of messages' >{{message}}<li>
</ul>

<input type="text" [(ngModel)]="message" >
<button (click)="send()">send!</button>
`,
})
@Injectable()
export class UpdateComponent {
    connection: HubConnection;

    messages: string[];

    message: string;

    error;

    constructor() { 
        this.messages = [];
        
        this.connection = new HubConnection(`http://${document.location.host}/updates`, 'formatType=json&format=text');

        this.connection.on('Send', (message) => {
            this.messages.push(message);
        });

        this.connection.start();
    }

    async send() {
        try {
            this.error = "";
            await this.connection.invoke('Send', this.message);
            this.message = "";
        } catch (error) {
            this.error = error;
        }
    }
}