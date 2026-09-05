import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DashboardService } from '../../../core/services/dashboard.service';
import { CustomerDashboard } from '../../../core/models/dashboard.models';


@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  templateUrl: './customer-dashboard.html',
  styleUrl: './customer-dashboard.css'
})
export class CustomerDashboardComponent implements OnInit {


  private readonly dashboardService =
    inject(DashboardService);

  private readonly cdr = inject(ChangeDetectorRef);


  dashboard: CustomerDashboard | null = null;


  ngOnInit(): void {

    this.dashboardService
      .getCustomerDashboard()
      .subscribe({

        next: (response) => {

          console.log(
            'CUSTOMER DASHBOARD:',
            response
          );

          this.dashboard = response;
          this.cdr.detectChanges();

        },


        error: (error) => {

          console.error(
            'Customer dashboard failed',
            error
          );

        }

      });

  }

}



