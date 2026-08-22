import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { isSafeReturnUrl } from './return-url';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  const returnUrl = route.queryParamMap.get('returnUrl');
  if (isSafeReturnUrl(returnUrl) && auth.hasAnyRole(['Student'])) {
    return router.parseUrl(returnUrl);
  }

  return router.createUrlTree([auth.homeForCurrentUser()]);
};
