import { Component, inject } from '@angular/core';
import {
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './customer-layout.html',
  styleUrl: './customer-layout.css'
})
export class CustomerLayoutComponent {
  private readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}