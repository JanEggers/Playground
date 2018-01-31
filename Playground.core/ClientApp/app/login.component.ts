import { 
    Component, 
    Injectable,
    Http,
    Headers,
} from "./vendor";

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

<p *ngIf="error" >{{error}}</p>
`
})
export class LoginComponent {

    constructor(public http: Http) {
        this.user = "someone";
        this.password = "pass";
    }

    user: string;
    password: string;

    error: string;

    async login(): Promise<any> {
        try {

            var headers = new Headers();
            headers.append("Content-Type", "application/x-www-form-urlencoded");

            var request: string = "/connect/token";

            var body = "grant_type=password&scope=offline_access&username=" + this.user + "&password=" + this.password;

            var response = await this.http.post(request, body, {
                headers: headers,
            }).toPromise();

            var data = response.json();
        } catch (error) {
            this.handleError(error);
        }
    }

    private handleError(error: any): Promise<void> {
        this.error = error.message || error;
        throw error;
    }
}