import { Component, Injectable } from "@angular/core";
import { Http } from "@angular/http";

import "rxjs/add/operator/toPromise";

@Component({
    selector: "login",
    template: `
<h2>Login</h2>
<div>
    <label>user: </label>
    <input [(ngModel)]="user" placeholder="user">
</div>
<div>
    <label>password: </label>
    <input [(ngModel)]="password" placeholder="password">
</div>
<button (click)="login()" >Login</button>
`,
})

@Injectable()
export class LoginComponent {

    constructor(private http: Http) {
    }

    user: string;
    password: string;

    login(): Promise<void> {
        var request: string = "/Account/Login?userName=" + this.user + "&password=" + this.password;

        return this.http.post(request, {})
            .toPromise()
            .then(response => response.json().data)
            .then(d => {
                return d;
            })
            .catch(this.handleError);

        //this.user.toString();
    }

    private handleError(error: any): Promise<void> {
        console.error("An error occurred", error);
        return Promise.reject(error.message || error);
    }
}