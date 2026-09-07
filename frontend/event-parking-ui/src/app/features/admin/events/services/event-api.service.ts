import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  CreateEventRequest,
  EventFilters,
  EventItem,
  UpdateEventRequest
} from '../models/event.model';

@Injectable({
  providedIn: 'root'
})
export class EventApiService {
  private readonly http = inject(HttpClient);

  getAll(filters: EventFilters = {}): Observable<EventItem[]> {
    let params = new HttpParams();

    if (filters.search?.trim()) {
      params = params.set('search', filters.search.trim());
    }

    if (filters.dateFromUtc) {
      params = params.set('dateFromUtc', filters.dateFromUtc);
    }

    if (filters.dateToUtc) {
      params = params.set('dateToUtc', filters.dateToUtc);
    }

    if (filters.venueId !== undefined) {
      params = params.set('venueId', filters.venueId.toString());
    }

    if (filters.categoryId !== undefined) {
      params = params.set(
        'categoryId',
        filters.categoryId.toString()
      );
    }

    return this.http.get<EventItem[]>(
      `${environment.apiBaseUrl}/events`,
      { params }
    );
  }

  getById(eventId: number): Observable<EventItem> {
    return this.http.get<EventItem>(
      `${environment.apiBaseUrl}/events/${eventId}`
    );
  }

  create(request: CreateEventRequest): Observable<EventItem> {
    return this.http.post<EventItem>(
      `${environment.apiBaseUrl}/admin/events`,
      request
    );
  }

  update(
    eventId: number,
    request: UpdateEventRequest
  ): Observable<EventItem> {
    return this.http.put<EventItem>(
      `${environment.apiBaseUrl}/admin/events/${eventId}`,
      request
    );
  }

  delete(eventId: number): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiBaseUrl}/admin/events/${eventId}`
    );
  }
}
