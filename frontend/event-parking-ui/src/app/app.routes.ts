import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(module => module.Login)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register')
        .then(module => module.Register)
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./features/auth/verify-email/verify-email')
        .then(module => module.VerifyEmail)
  },
  {
    path: 'resend-verification',
    loadComponent: () =>
      import(
        './features/auth/resend-verification/resend-verification'
      ).then(module => module.ResendVerification)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import(
        './features/auth/forgot-password/forgot-password'
      ).then(module => module.ForgotPassword)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import(
        './features/auth/reset-password/reset-password'
      ).then(module => module.ResetPassword)
  },
  {
    path: 'customer/profile',
    canActivate: [authGuard],
    data: {
      roles: ['Customer']
    },
    loadComponent: () =>
      import(
        './features/customer/profile/customer-profile'
      ).then(module => module.CustomerProfile)
  },
  {
    path: 'admin/customers',
    canActivate: [authGuard],
    data: {
      roles: ['Administrator']
    },
    loadComponent: () =>
      import(
        './features/admin/customers/admin-customers'
      ).then(module => module.AdminCustomers)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
