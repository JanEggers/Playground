/// <reference path="../../typings/es6-shim/es6-shim.d.ts" />

import { NgModule } from "@angular/core";
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule }   from '@angular/forms';
import { HttpModule }    from '@angular/http';

import { AppComponent }   from './app.component';
import { LoginComponent } from "./login.component";

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule
    ],
    declarations: [
        AppComponent,
        LoginComponent
    ],
    providers: [
    ],
    bootstrap: [AppComponent]
})

export class AppModule {
}