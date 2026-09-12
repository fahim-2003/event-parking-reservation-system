import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit
} from '@angular/core';

import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

import {
  AdminDashboard
} from '../../../core/models/dashboard.models';

import {
  DashboardService
} from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    DecimalPipe
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboardComponent implements OnInit {

  private readonly dashboardService =
    inject(DashboardService);

  private readonly cdr =
    inject(ChangeDetectorRef);

  dashboard:
    AdminDashboard | null = null;

  loading = true;

  ngOnInit(): void {

    this.dashboardService
      .getAdminDashboard()
      .subscribe({

        next: response => {

          this.dashboard = response;
          this.loading = false;

          this.cdr.detectChanges();
        },

        error: error => {

          console.error(
            'Dashboard loading failed',
            error
          );

          this.loading = false;

          this.cdr.detectChanges();
        }
      });
  }
}