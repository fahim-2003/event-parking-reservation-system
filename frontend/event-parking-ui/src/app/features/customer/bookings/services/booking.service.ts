import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  BookingResponse,
  CreateBookingRequest
} from '../models/booking.models';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly http = inject(HttpClient);

  create(
    request: CreateBookingRequest
  ): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(
      `${environment.apiBaseUrl}/customer/bookings`,
      request
    );
  }
}