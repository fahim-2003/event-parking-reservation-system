import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  BookingResponse,
  CreateBookingRequest
} from '../models/booking.models';

export interface SeatResponse {
  id: number;
  eventId: number;
  rowLabel: string;
  seatNumber: number;
  displayLabel: string;
  status: string;
  rowVersion: string;
}

export interface ParkingSlotResponse {
  id: number;
  eventId: number;
  zone: string;
  slotNumber: number;
  status: string;
  rowVersion: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {

  private readonly http = inject(HttpClient);

  private readonly bookingApiUrl =
    `${environment.apiBaseUrl}/customer/bookings`;

  create(
    request: CreateBookingRequest
  ): Observable<BookingResponse> {

    return this.http.post<BookingResponse>(
      this.bookingApiUrl,
      request
    );
  }

  getMine(): Observable<BookingResponse[]> {

    return this.http.get<BookingResponse[]>(
      this.bookingApiUrl
    );
  }

  cancel(
    bookingId: number
  ): Observable<BookingResponse> {

    return this.http.post<BookingResponse>(
      `${this.bookingApiUrl}/${bookingId}/cancel`,
      {}
    );
  }

  getSeats(
    eventId: number
  ): Observable<SeatResponse[]> {

    return this.http.get<SeatResponse[]>(
      `${environment.apiBaseUrl}/events/${eventId}/seats`
    );
  }

  getParkingSlots(
    eventId: number
  ): Observable<ParkingSlotResponse[]> {

    return this.http.get<ParkingSlotResponse[]>(
      `${environment.apiBaseUrl}/events/${eventId}/parking-slots`
    );
  }
}
