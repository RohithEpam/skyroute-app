import { Routes } from '@angular/router';

export const routes: Routes = [
     { path: '', loadComponent: () => import('./pages/search/search.component').then(m => m.SearchComponent) },
  { path: 'results', loadComponent: () => import('./pages/results/results.component').then(m => m.ResultsComponent) },
  { path: 'booking', loadComponent: () => import('./pages/booking/booking.component').then(m => m.BookingComponent) },
  { path: 'confirmation', loadComponent: () => import('./pages/confirmation/confirmation.component').then(m => m.ConfirmationComponent) },
  {path: 'my-booking', 
  loadComponent: () => import('./pages/booking-details/booking-details.component')
    .then(m => m.BookingDetailsComponent) },
  { path: '**', redirectTo: '' }
];
