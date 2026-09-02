import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  GenerateParkingLayoutRequest,
  ParkingSlot,
  UpdateParkingSlotRequest
} from '../models/parking-slot.model';

@Injectable({
  providedIn: 'root'
})
export class ParkingApiService {
  private readonly http = inject(HttpClient);

  getByEvent(eventId: number): Observable<ParkingSlot[]> {
    return this.http.get<ParkingSlot[]>(
      `${environment.apiBaseUrl}/events/${eventId}/parking-slots`
    );
  }

  generate(
    eventId: number,
    request: GenerateParkingLayoutRequest
  ): Observable<ParkingSlot[]> {
    return this.http.post<ParkingSlot[]>(
      `${environment.apiBaseUrl}/admin/events/${eventId}/parking-slots`,
      request
    );
  }

  update(
    eventId: number,
    parkingSlotId: number,
    request: UpdateParkingSlotRequest
  ): Observable<ParkingSlot> {
    return this.http.put<ParkingSlot>(
      `${environment.apiBaseUrl}/admin/events/${eventId}/parking-slots/${parkingSlotId}`,
      request
    );
  }

  delete(
    eventId: number,
    parkingSlotId: number,
    rowVersion: string
  ): Observable<void> {
    const params = new HttpParams()
      .set('rowVersion', rowVersion);

    return this.http.delete<void>(
      `${environment.apiBaseUrl}/admin/events/${eventId}/parking-slots/${parkingSlotId}`,
      { params }
    );
  }
}
