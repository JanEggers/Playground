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
`,
})

@Injectable()
export class LoginComponent {

    constructor(private http: Http) {
        this.user = "someone";
        this.password = "pass";
    }

    user: string;
    password: string;

    error: string;

    login(): Promise<any> {
        var headers = new Headers();
        headers.append("Content-Type", "application/x-www-form-urlencoded");

        var request: string = "/connect/token";

        var body = "grant_type=password&scope=offline_access&username=" + this.user + "&password=" + this.password;

        return this.http.post(request, body, {
            headers: headers,
        })
            .toPromise()
            .then(response => response.json())
            .then(d => {
                return d;
            })
            .catch((error) => this.handleError(error));
    }

    private handleError(error: any): Promise<void> {
        this.error = error.message || error;
        throw error;
    }
}