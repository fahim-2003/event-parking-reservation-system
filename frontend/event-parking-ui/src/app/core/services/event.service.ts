import { Injectable, inject } from '@angular/core';
import {
  HttpClient,
  HttpParams
} from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

export interface EventItem {
  id: number;
  name: string;
  description?: string;
  imageUrl?: string | null;
  venueId: number;
  venueName: string;
  venueAddress?: string;
  eventCategoryId: number;
  categoryName: string;
  startDateTimeUtc: string;
  endDateTimeUtc: string;
  ticketPrice: number;
  parkingFee: number;
  capacity: number;
}

export interface EventFilters {
  search?: string;
  dateFromUtc?: string;
  dateToUtc?: string;
  venueId?: number;
  categoryId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/events`;

  getEvents(filters: EventFilters = {}): Observable<EventItem[]> {
    let params = new HttpParams();

    const search = filters.search?.trim();

    if (search) {
      params = params.set('search', search);
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
      params = params.set('categoryId', filters.categoryId.toString());
    }

    return this.http.get<EventItem[]>(
      this.apiUrl,
      { params }
    );
  }

  getEventById(id: number): Observable<EventItem> {
    return this.http.get<EventItem>(
      `${this.apiUrl}/${id}`
    );
  }
}
