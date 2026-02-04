import { HttpInterceptorFn } from '@angular/common/http';
import { tap } from 'rxjs/operators';

export const loggingInterceptor: HttpInterceptorFn = (req, next) => {
  const startTime = Date.now();

  // Log the request
  console.log('%c📤 HTTP Request', 'color: #3B82F6; font-weight: bold', {
    method: req.method,
    url: req.url,
    body: req.body,
    headers: req.headers.keys().reduce((acc, key) => {
      acc[key] = req.headers.get(key);
      return acc;
    }, {} as any)
  });

  return next(req).pipe(
    tap({
      next: (event) => {
        // Log successful response
        if (event.type !== 0) { // Only log actual responses, not progress events
          const duration = Date.now() - startTime;
          console.log('%c✅ HTTP Response', 'color: #10B981; font-weight: bold', {
            method: req.method,
            url: req.url,
            status: (event as any).status,
            duration: `${duration}ms`,
            body: (event as any).body
          });
        }
      },
      error: (error) => {
        // Log error response
        const duration = Date.now() - startTime;
        console.error('%c❌ HTTP Error', 'color: #EF4444; font-weight: bold', {
          method: req.method,
          url: req.url,
          status: error.status,
          duration: `${duration}ms`,
          error: error.error,
          message: error.message
        });
      }
    })
  );
};
