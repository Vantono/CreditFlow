import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { Register } from './features/auth/register/register';
import { Dashboard } from './features/dashboard/dashboard';
import { BankerDashboardComponent } from './features/banker/banker-dashboard/banker-dashboard';
import { MainLayout } from './layout/main-layout/main-layout';
import { authGuard } from './core/auth/auth-guard';

export const routes: Routes = [
  // 1. Default -> Login
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // 2. Auth Routes (Public - no layout shell)
  {
    path: 'auth/login',
    component: LoginComponent
  },
  {
    path: 'auth/register',
    component: Register
  },

  // 3. Protected Routes (wrapped in MainLayout with header + footer)
  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: Dashboard
      },
      {
        path: 'banker',
        component: BankerDashboardComponent,
        canActivate: [authGuard],
        data: { role: 'Banker' }
      }
    ]
  },

  // 4. Wildcard -> Login
  { path: '**', redirectTo: 'auth/login' }
];
