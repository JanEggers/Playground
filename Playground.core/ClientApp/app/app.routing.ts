import { ModuleWithProviders }  from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoginComponent }       from './login.component';
import { RegisterComponent }    from './register.component';

const appRoutes: Routes = [
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'register',
        component: RegisterComponent
    },
    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full'
    },
];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);