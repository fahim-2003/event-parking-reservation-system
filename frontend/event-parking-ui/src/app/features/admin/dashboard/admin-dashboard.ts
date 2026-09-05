import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';

import { DashboardService } from '../../../core/services/dashboard.service';
import { AdminDashboard } from '../../../core/models/dashboard.models';


@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboardComponent implements OnInit {

  private readonly dashboardService = inject(DashboardService);

  private readonly cdr = inject(ChangeDetectorRef);

  dashboard: AdminDashboard | null = null;

  loading = true;


  ngOnInit(): void {

    this.dashboardService
      .getAdminDashboard()
      .subscribe({

        next: (response) => {

          console.log(
            'DASHBOARD RESPONSE:',
            response
          );

          this.dashboard = response;
          console.log('DASHBOARD STATE:', this.dashboard);
          this.cdr.detectChanges();
          this.loading = false;

        },

        error: (error) => {

          console.error(
            'Dashboard loading failed',
            error
          );

          this.loading = false;

        }

      });

  }

}

