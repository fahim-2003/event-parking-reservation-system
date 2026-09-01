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
  }
];
