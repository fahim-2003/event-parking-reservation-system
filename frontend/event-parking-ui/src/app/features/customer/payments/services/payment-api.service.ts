import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';

export interface PaymentResponse {
  id: number;
  bookingId: number;
  amount: number;
  paymentMethod: string;
  status: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentApiService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiBaseUrl}/payments`;

  process(
    bookingId: number,
    paymentMethod: string
  ): Observable<PaymentResponse> {

    return this.http.post<PaymentResponse>(
      this.apiUrl,
      {
        bookingId,
        paymentMethod
      }
    );
  }

  getMine(): Observable<PaymentResponse[]> {

    return this.http.get<PaymentResponse[]>(
      `${this.apiUrl}/mine`
    );
  }
}
