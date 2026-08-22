import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { MANAGE_USERS_PERMISSION } from './auth.models';

export const manageUsersGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (auth.canManageUsers()) {
    return true;
  }

  return router.createUrlTree([auth.homeForCurrentUser()]);
};

export const permissionGuard = (permission: string): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/login']);
    }

    if (auth.hasPermission(permission)) {
      return true;
    }

    return router.createUrlTree([auth.homeForCurrentUser()]);
  };
};

export { MANAGE_USERS_PERMISSION };
