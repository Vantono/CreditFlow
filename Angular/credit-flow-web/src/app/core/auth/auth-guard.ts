import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  // 1. Αν δεν είναι συνδεδεμένος -> Login με "μνήμη" (returnUrl)
  if (!authStore.isAuthenticated()) {
    
    // ΕΔΩ ΧΡΗΣΙΜΟΠΟΙΟΥΜΕ ΤΟ state!
    // Λέμε στο router: Πήγαινε στο login, αλλά βάλε στο URL και το ?returnUrl=...
    router.navigate(['/auth/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    
    return false;
  }

  // 2. Έλεγχος Ρόλων (όπως πριν)
  const requiredRole = route.data?.['role'];
  
  if (requiredRole === 'Banker' && !authStore.isBanker()) {
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};