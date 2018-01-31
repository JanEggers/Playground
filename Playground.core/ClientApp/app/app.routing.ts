import { Routes } from './vendor';

import { LoginComponent }       from './login.component';
import { RegisterComponent }    from './register.component';
import { UpdateComponent }      from './update.component';

export const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'register',
        component: RegisterComponent
    },
    {
        path: 'updates',
        component: UpdateComponent
    },
    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full'
    },
];