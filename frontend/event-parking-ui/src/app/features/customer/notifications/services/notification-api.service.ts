import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';

export interface CustomerNotification {
  id: number;
  message: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationApiService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiBaseUrl}/customer/notifications`;

  getMine(): Observable<CustomerNotification[]> {
    return this.http.get<CustomerNotification[]>(
      this.apiUrl
    );
  }

  markRead(id: number): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${id}/read`,
      {}
    );
  }
}
