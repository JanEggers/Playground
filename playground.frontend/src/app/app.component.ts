import { Component } from '@angular/core';

@Component({
    selector: "playground",
    template: `
<h1>Playground</h1>
   <a routerLink="/login">Login</a>
   <a routerLink="/register">Register</a>
   <a routerLink="/updates">Updates</a>
   <router-outlet></router-outlet>
`,
})
export class AppComponent {
}