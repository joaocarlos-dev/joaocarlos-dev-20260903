import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthSession } from './auth-session';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(AuthSession);
  const router = inject(Router);
  const isApiRequest = request.url.startsWith('/api/');
  const isLoginRequest = request.url === '/api/v1/auth/login';
  const token = session.token();
  const authenticatedRequest = isApiRequest && !isLoginRequest && token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (isApiRequest && !isLoginRequest && error instanceof HttpErrorResponse && error.status === 401) {
        const returnUrl = router.url !== '/login' ? router.url : undefined;
        session.clear();
        void router.navigate(['/login'], { queryParams: { expired: 1, returnUrl } });
      }

      return throwError(() => error);
    }),
  );
};
