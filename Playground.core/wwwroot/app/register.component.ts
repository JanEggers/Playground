import { Component, Injectable } from "@angular/core";
import { Http } from "@angular/http";

import "rxjs/add/operator/toPromise";

@Component({
    selector: "register",
    template: `
<h2>Register</h2>
<div>
    <label>user: </label>
    <input [(ngModel)]="user" placeholder="user">
</div>
<div>
    <label>password: </label>
    <input [(ngModel)]="password" placeholder="password">
</div>
<button (click)="login()" >Register</button>

<p *ngIf="error" >{{error}}</p>
`,
})

@Injectable()
export class RegisterComponent {

    constructor(private http: Http) {
        this.user = "someone";
        this.password = "pass";
    }

    user: string;
    password: string;

    error: string;
       

    login(): Promise<any> {
        var request: string = "/Account/Register?userName=" + this.user + "&password=" + this.password;

        return this.http.post(request, {})
            .toPromise()
            .catch((error) => this.handleError(error));
    }

    private handleError(error: any): Promise<void> {
        this.error = error.message || error;
        return Promise.reject(error);
    }
}