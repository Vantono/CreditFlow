import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { loggingInterceptor } from './core/interceptors/logging.interceptor';
import { errorHandlingInterceptor } from './core/interceptors/error-handling.interceptor';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura'

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),// Για το PrimeNG
    // 2. ΕΝΕΡΓΟΠΟΙΗΣΗ HTTP
    provideHttpClient(withFetch(), withInterceptors([loggingInterceptor, errorHandlingInterceptor, authInterceptor])),
    providePrimeNG({
        theme: {
            preset: Aura,
            options: {
                darkModeSelector: false || 'none' // Αν θέλουμε να απενεργοποιήσουμε το dark mode αρχικά
            }
        }
    })
  ]
};
