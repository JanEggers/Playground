import { Component } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from '@aspnet/signalr';

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

        this.connection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Trace)
            .withUrl("/updates", HttpTransportType.WebSockets)
            .build();

        this.connection.on('Send', (message) => {
            this.messages.push(message);
        });

        this.start = this.connection.start();
        this.stream();
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

    async stream() {
        try {
            await this.start;
            this.error = "";
            var count = 0;
            var sub = this.connection.stream<Date>('Tick').subscribe({
                closed: false,
                next: (item) => {
                    count++;
                    this.messages.push(item.toString());

                    if (count > 5)
                    {
                        sub.dispose();
                    }
                },
                error: (err) => {
                    this.messages.push(err);
                },
                complete: () => {
                    this.messages.push('completed');
                }
            });
            this.message = "";
        } catch (error) {
            this.error = error;
        }
    }
}