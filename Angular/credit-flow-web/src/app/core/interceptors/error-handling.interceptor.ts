import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorHandlingInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Enhanced error object with standardized structure
      const enhancedError = {
        status: error.status,
        statusText: error.statusText,
        url: error.url,
        timestamp: new Date().toISOString(),
        error: error.error,
        message: '',
        errors: [] as string[]
      };

      // Handle different error status codes
      switch (error.status) {
        case 400: // Bad Request
          if (typeof error.error === 'string') {
            enhancedError.message = error.error;
            enhancedError.errors = [error.error];
          } else if (Array.isArray(error.error)) {
            // ASP.NET Identity errors array
            enhancedError.errors = error.error.map((e: any) => e.description || e.code || 'Validation error');
            enhancedError.message = enhancedError.errors.join(' ');
          } else if (error.error?.errors) {
            // ASP.NET ModelState errors
            const errorMessages: string[] = [];
            for (const key in error.error.errors) {
              errorMessages.push(...error.error.errors[key]);
            }
            enhancedError.errors = errorMessages;
            enhancedError.message = errorMessages.join(' ');
          } else {
            enhancedError.message = 'Bad request. Please check your input.';
            enhancedError.errors = [enhancedError.message];
          }
          break;

        case 401: // Unauthorized
          enhancedError.message = 'Authentication required. Please login.';
          enhancedError.errors = [enhancedError.message];

          // Log the user out and redirect to login
          console.warn('🔒 401 Unauthorized - Redirecting to login');
          localStorage.removeItem('token');
          localStorage.removeItem('user');

          // Only redirect if not already on login/register page
          if (!router.url.includes('/auth/')) {
            router.navigate(['/auth/login'], {
              queryParams: { returnUrl: router.url }
            });
          }
          break;

        case 403: // Forbidden
          enhancedError.message = 'Access denied. You do not have permission to perform this action.';
          enhancedError.errors = [enhancedError.message];
          break;

        case 404: // Not Found
          enhancedError.message = 'The requested resource was not found.';
          enhancedError.errors = [enhancedError.message];
          break;

        case 500: // Internal Server Error
          enhancedError.message = 'Server error. Please try again later.';
          enhancedError.errors = [enhancedError.message];
          console.error('🔥 500 Internal Server Error:', error);
          break;

        case 503: // Service Unavailable
          enhancedError.message = 'Service temporarily unavailable. Please try again later.';
          enhancedError.errors = [enhancedError.message];
          break;

        case 0: // Network error
          enhancedError.message = 'Network error. Please check your connection.';
          enhancedError.errors = [enhancedError.message];
          break;

        default:
          enhancedError.message = error.message || 'An unexpected error occurred.';
          enhancedError.errors = [enhancedError.message];
          break;
      }

      // Log enhanced error to console
      console.error('%c❌ HTTP Error Handler', 'color: #EF4444; font-weight: bold', {
        status: enhancedError.status,
        url: enhancedError.url,
        message: enhancedError.message,
        errors: enhancedError.errors,
        rawError: error.error
      });

      // Return enhanced error
      return throwError(() => ({
        ...error,
        enhancedError
      }));
    })
  );
};
