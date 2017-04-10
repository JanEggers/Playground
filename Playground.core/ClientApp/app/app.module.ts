﻿import { 
    NgModule, 
    LocationStrategy, 
    HashLocationStrategy,
    FormsModule,
    HttpModule,
} from "./vendor";

import { BrowserModule } from '@angular/platform-browser';
import { AppComponent }   from './app.component';
import { routing } from './app.routing';
import { LoginComponent } from "./login.component";
import { RegisterComponent } from "./register.component";

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        routing
    ],
    declarations: [
        AppComponent,
        LoginComponent,
        RegisterComponent
    ],
    providers: [
        { provide:LocationStrategy, useClass: HashLocationStrategy }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}