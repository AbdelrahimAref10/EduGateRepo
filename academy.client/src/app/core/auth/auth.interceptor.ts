import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';

let refreshInFlight: ReturnType<AuthService['refresh']> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  const auth = inject(AuthService);

  const isAuthEndpoint =
    req.url.includes('/api/auth/login') ||
    req.url.includes('/api/auth/register') ||
    req.url.includes('/api/auth/refresh');

  const token = tokenStorage.getAccessToken();
  const authReq =
    token && !isAuthEndpoint
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      if (!refreshInFlight) {
        refreshInFlight = auth.refresh().pipe(
          catchError((refreshError) => {
            refreshInFlight = null;
            auth.logout();
            return throwError(() => refreshError);
          }),
        );
      }

      return refreshInFlight.pipe(
        switchMap((session) => {
          refreshInFlight = null;
          if (!session?.accessToken) {
            auth.logout();
            return throwError(() => error);
          }

          const retry = req.clone({
            setHeaders: { Authorization: `Bearer ${session.accessToken}` },
          });
          return next(retry);
        }),
      );
    }),
  );
};
