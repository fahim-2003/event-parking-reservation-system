import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  CreateVenueRequest,
  UpdateVenueRequest,
  Venue
} from '../models/venue.model';

@Injectable({
  providedIn: 'root'
})
export class VenueApiService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Venue[]> {
    return this.http.get<Venue[]>(
      `${environment.apiBaseUrl}/venues`
    );
  }

  getById(venueId: number): Observable<Venue> {
    return this.http.get<Venue>(
      `${environment.apiBaseUrl}/venues/${venueId}`
    );
  }

  create(request: CreateVenueRequest): Observable<Venue> {
    return this.http.post<Venue>(
      `${environment.apiBaseUrl}/admin/venues`,
      request
    );
  }

  update(
    venueId: number,
    request: UpdateVenueRequest
  ): Observable<Venue> {
    return this.http.put<Venue>(
      `${environment.apiBaseUrl}/admin/venues/${venueId}`,
      request
    );
  }

  delete(venueId: number): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiBaseUrl}/admin/venues/${venueId}`
    );
  }
}