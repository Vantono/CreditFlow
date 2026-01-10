import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { authGuard } from './core/auth/auth-guard';
// import { RegisterComponent } from './features/auth/register/register.component';
// import { DashboardComponent } from './features/dashboard/dashboard.component';
// import { LoanApplicationComponent } from './features/loan-application/loan-application.component';
// import { BankerDashboardComponent } from './features/banker/banker-dashboard/banker-dashboard.component'; // Τσέκαρε αν το path είναι ακριβώς έτσι

export const routes: Routes = [
  // 1. Default: Αν μπει σκέτο στο domain -> πήγαινε Login
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // 2. Auth Routes (Δημόσια)
  { 
    path: 'auth/login', 
    component: LoginComponent 
  },
//   { 
//     path: 'auth/register', 
//     component: RegisterComponent 
//   },

//   // 3. Customer Routes (Προστατευμένα)
//   {
//     path: 'dashboard',
//     component: DashboardComponent,
//     canActivate: [authGuard] // Ο Guard ελέγχει αν είσαι logged in
//   },
//   {
//     path: 'loan-application',
//     component: LoanApplicationComponent,
//     canActivate: [authGuard]
//   },

//   // 4. Banker Routes (Προστατευμένα + Ρόλος)
//   {
//     path: 'banker',
//     component: BankerDashboardComponent,
//     canActivate: [authGuard],
//     data: { role: 'Banker' } // <--- Εδώ λέμε στον Guard να ψάξει για ρόλο 'Banker'
//   },

  // 5. Wildcard: Αν γράψει κάτι άκυρο -> πήγαινε Login
  { path: '**', redirectTo: 'auth/login' }
];