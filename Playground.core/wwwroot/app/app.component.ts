import { Component } from "@angular/core";

import { LoginComponent } from "./login.component";

@Component({
    selector: "playground",
    directives: [LoginComponent],
    template: `
<h1>Playground</h1>
<login></login>
`,
})

export class AppComponent { }