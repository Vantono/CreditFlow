import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  if (!authStore.isAuthenticated()) {
    
    router.navigate(['/auth/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    
    return false;
  }

  const requiredRole = route.data?.['role'];
  
  if (requiredRole === 'Banker' && !authStore.isBanker()) {
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};