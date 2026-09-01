import {
  HttpClient,
  HttpParams
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminCustomerSummary,
  CustomerProfile,
  UpdateCustomerProfileRequest
} from '../models/customer.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly http = inject(HttpClient);

  getOwnProfile(): Observable<CustomerProfile> {
    return this.http.get<CustomerProfile>(
      `${environment.apiBaseUrl}/customers/me`
    );
  }

  updateOwnProfile(
    request: UpdateCustomerProfileRequest
  ): Observable<CustomerProfile> {
    return this.http.put<CustomerProfile>(
      `${environment.apiBaseUrl}/customers/me`,
      request
    );
  }

  searchCustomers(
    search = ''
  ): Observable<AdminCustomerSummary[]> {
    let params = new HttpParams();

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<AdminCustomerSummary[]>(
      `${environment.apiBaseUrl}/customers`,
      { params }
    );
  }

  getCustomerForAdmin(
    customerId: string
  ): Observable<AdminCustomerSummary> {
    return this.http.get<AdminCustomerSummary>(
      `${environment.apiBaseUrl}/customers/${customerId}`
    );
  }
}
