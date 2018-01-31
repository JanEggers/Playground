import { 
    NgModule, 
    LocationStrategy, 
    HashLocationStrategy,
    FormsModule,
    HttpModule,
    BrowserModule,
    RouterModule,
} from "./vendor";

import { routes } from './app.routing';

import { AppComponent } from './app.component';
import { LoginComponent } from "./login.component";
import { RegisterComponent } from "./register.component";
import { UpdateComponent } from "./update.component";

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        RouterModule.forRoot(routes)
    ],
    declarations: [
        AppComponent,
        LoginComponent,
        RegisterComponent,
        UpdateComponent
    ],
    providers: [
        { provide:LocationStrategy, useClass: HashLocationStrategy }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}