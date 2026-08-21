import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AppRoleName } from './auth.models';
import { AuthService } from './auth.service';

export const roleGuard = (allowed: AppRoleName[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/login']);
    }

    if (auth.hasAnyRole(allowed)) {
      return true;
    }

    return router.createUrlTree([auth.homeForCurrentUser()]);
  };
};
