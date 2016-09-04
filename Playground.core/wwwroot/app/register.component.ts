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
`,
})

@Injectable()
export class RegisterComponent {

    constructor(private http: Http) {
    }

    user: string;
    password: string;
       

    login(): Promise<void> {
        var request: string = "/Account/Register?userName=" + this.user + "&password=" + this.password;

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