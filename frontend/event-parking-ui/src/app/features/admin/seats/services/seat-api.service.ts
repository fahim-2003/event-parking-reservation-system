import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  GenerateSeatMapRequest,
  Seat,
  UpdateSeatRequest
} from '../models/seat.model';

@Injectable({
  providedIn: 'root'
})
export class SeatApiService {
  private readonly http = inject(HttpClient);

  getByEvent(eventId: number): Observable<Seat[]> {
    return this.http.get<Seat[]>(
      `${environment.apiBaseUrl}/events/${eventId}/seats`
    );
  }

  generate(
    eventId: number,
    request: GenerateSeatMapRequest
  ): Observable<Seat[]> {
    return this.http.post<Seat[]>(
      `${environment.apiBaseUrl}/admin/events/${eventId}/seats`,
      request
    );
  }

  update(
    eventId: number,
    seatId: number,
    request: UpdateSeatRequest
  ): Observable<Seat> {
    return this.http.put<Seat>(
      `${environment.apiBaseUrl}/admin/events/${eventId}/seats/${seatId}`,
      request
    );
  }
}
