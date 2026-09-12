import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [

  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(m => m.Login)
  },

  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register')
        .then(m => m.Register)
  },


  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password')
        .then(m => m.ForgotPassword)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password')
        .then(m => m.ResetPassword)
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    data: {
      roles: ['Administrator']
    },

    loadComponent: () =>
      import('./features/admin/layout/admin-layout/admin-layout')
        .then(m => m.AdminLayoutComponent),

    children: [

      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/admin/dashboard/admin-dashboard')
            .then(m => m.AdminDashboardComponent)
      },

      {
        path: 'events',
        loadComponent: () =>
          import('./features/admin/events/pages/event-management/event-management')
            .then(m => m.EventManagement)
      },

      {
        path: 'venues',
        loadComponent: () =>
          import('./features/admin/venues/pages/venue-management/venue-management')
            .then(m => m.VenueManagement)
      },

      {
        path: 'categories',
        loadComponent: () =>
          import('./features/admin/categories/pages/category-management/category-management')
            .then(m => m.CategoryManagement)
      },

      {
        path: 'seats',
        loadComponent: () =>
          import('./features/admin/seats/pages/seat-layout/seat-layout')
            .then(m => m.SeatLayout)
      },

      {
        path: 'parking',
        loadComponent: () =>
          import('./features/admin/parking/pages/parking-layout/parking-layout')
            .then(m => m.ParkingLayout)
      },

      {
        path: 'customers',
        loadComponent: () =>
          import('./features/admin/customers/admin-customers')
            .then(m => m.AdminCustomers)
      },

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }

    ]
  },


  {
    path: 'customer',
    canActivate: [authGuard],
    data: {
      roles: ['Customer']
    },

    loadComponent: () =>
      import('./features/customer/layout/customer-layout/customer-layout')
        .then(m => m.CustomerLayoutComponent),

    children: [

      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/customer/dashboard/customer-dashboard')
            .then(m => m.CustomerDashboardComponent)
      },


      {
        path: 'events',
        loadComponent: () =>
          import('./features/customer/events/customer-events/customer-events')
            .then(m => m.CustomerEventsComponent)
      },


      {
        path: 'profile',
        loadComponent: () =>
          import('./features/customer/profile/customer-profile')
            .then(m => m.CustomerProfile)
      },
      {
        path: 'bookings/create/:eventId',
        loadComponent: () =>
          import('./features/customer/bookings/pages/booking-create/booking-create')
            .then(m => m.BookingCreate)
      },


      {
        path: 'bookings/summary',
        loadComponent: () =>
          import('./features/customer/bookings/pages/booking-summary/booking-summary')
            .then(m => m.BookingSummary)
      },



      {
        path: 'payments',
        loadComponent: () =>
          import('./features/customer/payments/pages/payment-history/payment-history')
            .then(m => m.PaymentHistory)
      },


      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/customer/notifications/pages/notification-list/notification-list')
            .then(m => m.NotificationList)
      },



      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }

    ]
  },


  {
    path: '',
    loadComponent: () =>
      import('./features/landing/landing')
        .then(m => m.Landing)
  },


  {
    path: '**',
    redirectTo: ''
  }

];
