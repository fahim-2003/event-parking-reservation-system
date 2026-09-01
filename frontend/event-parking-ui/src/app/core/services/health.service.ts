import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HealthService {
  private readonly http = inject(HttpClient);

  getHealth(): Observable<string> {
    return this.http.get(
      `${environment.apiBaseUrl}/health`,
      { responseType: 'text' }
    );
  }
}