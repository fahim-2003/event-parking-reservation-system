import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router
} from '@angular/router';
import { AppRole } from '../models/auth.models';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (
  route,
  state
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(
      ['/login'],
      {
        queryParams: {
          returnUrl: state.url
        }
      }
    );
  }

  const allowedRoles =
    route.data['roles'] as AppRole[] | undefined;

  const currentRole = authService.role();

  if (
    allowedRoles?.length &&
    (
      !currentRole ||
      !allowedRoles.includes(currentRole)
    )
  ) {
    if (currentRole === 'Administrator') {
      return router.createUrlTree(['/admin/customers']);
    }

    if (currentRole === 'Customer') {
      return router.createUrlTree(['/customer/profile']);
    }

    return router.createUrlTree(['/login']);
  }

  return true;
};
