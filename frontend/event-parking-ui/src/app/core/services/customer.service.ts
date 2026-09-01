import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
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
}
