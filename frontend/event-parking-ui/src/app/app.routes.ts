import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'admin/venues',
    loadComponent: () =>
      import(
        './features/admin/venues/pages/venue-management/venue-management'
      ).then(component => component.VenueManagement)
  },
  {
    path: 'admin/categories',
    loadComponent: () =>
      import(
        './features/admin/categories/pages/category-management/category-management'
      ).then(component => component.CategoryManagement)
  },
  {
    path: 'admin/events',
    loadComponent: () =>
      import(
        './features/admin/events/pages/event-management/event-management'
      ).then(component => component.EventManagement)
  },
  {
    path: 'admin/seats',
    loadComponent: () =>
      import(
        './features/admin/seats/pages/seat-layout/seat-layout'
      ).then(component => component.SeatLayout)
  }
];
