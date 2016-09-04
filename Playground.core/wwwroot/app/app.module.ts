/// <reference path="../../typings/es6-shim/es6-shim.d.ts" />

import { NgModule } from "@angular/core";
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule }   from '@angular/forms';
import { HttpModule }    from '@angular/http';

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
    ],
    bootstrap: [AppComponent]
})

export class AppModule {
}