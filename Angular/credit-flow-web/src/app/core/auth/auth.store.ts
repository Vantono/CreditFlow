import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthResponse } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AuthStore {
  private router = inject(Router);
  
  // 1. STATE: Το ιδιωτικό signal που κρατάει τα δεδομένα
  // Αρχικοποιείται διαβάζοντας από το localStorage (αν υπάρχει saved login)
  private _user = signal<AuthResponse | null>(this.getUserFromStorage());

  // 2. SELECTORS: Τα δημόσια read-only signals που βλέπουν τα components
  
  // Το τρέχον αντικείμενο χρήστη
  readonly user = this._user.asReadonly();
  
  // Είναι συνδεδεμένος; (True αν υπάρχει user)
  readonly isAuthenticated = computed(() => !!this._user());

  // Το Token (για τους interceptors)
  readonly token = computed(() => this._user()?.token || null);

  // Είναι Τραπεζικός; (Αποκωδικοποιούμε το Token για να βρούμε τον ρόλο)
  readonly isBanker = computed(() => {
    const token = this.token();
    if (!token) return false;
    return this.hasRole(token, 'Banker');
  });

  // ==========================================================
  // ACTIONS (Methods που αλλάζουν το State)
  // ==========================================================

  login(authData: AuthResponse) {
    this._user.set(authData);

    localStorage.setItem('creditflow_user', JSON.stringify(authData));

    setTimeout(() => {
      if (this.isBanker()) {
        this.router.navigate(['/banker']);
      } else {
        this.router.navigate(['/dashboard']);
      }
    }, 0);
  }

  logout() {
    this._user.set(null);
    localStorage.removeItem('creditflow_user');
    this.router.navigate(['/auth/login']);
  }

  // ==========================================================
  // HELPERS (Private)
  // ==========================================================

  private getUserFromStorage(): AuthResponse | null {
    const stored = localStorage.getItem('creditflow_user');
    return stored ? JSON.parse(stored) : null;
  }

  // Μια απλή συνάρτηση για να διαβάζουμε claims από το JWT χωρίς εξωτερική βιβλιοθήκη
  private hasRole(token: string, role: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Το claim των ρόλων στο .NET Identity είναι συνήθως:
      // "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ή απλά "role"
      const userRoles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'];
      
      if (Array.isArray(userRoles)) {
        return userRoles.includes(role);
      }
      return userRoles === role;
    } catch (e) {
      return false;
    }
  }
}