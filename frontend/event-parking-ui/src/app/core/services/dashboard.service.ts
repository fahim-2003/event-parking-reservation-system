import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import {
  AdminDashboard,
  CustomerDashboard
} from '../models/dashboard.models';


@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5184/api/dashboard';


  getAdminDashboard() {
    return this.http.get<AdminDashboard>(
      `${this.apiUrl}/admin`
    );
  }


  getCustomerDashboard() {
    return this.http.get<CustomerDashboard>(
      `${this.apiUrl}/customer`
    );
  }
}
