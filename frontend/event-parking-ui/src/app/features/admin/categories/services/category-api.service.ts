import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  CreateCategoryRequest,
  EventCategory,
  UpdateCategoryRequest
} from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryApiService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<EventCategory[]> {
    return this.http.get<EventCategory[]>(
      `${environment.apiBaseUrl}/categories`
    );
  }

  create(request: CreateCategoryRequest): Observable<EventCategory> {
    return this.http.post<EventCategory>(
      `${environment.apiBaseUrl}/admin/categories`,
      request
    );
  }

  update(
    categoryId: number,
    request: UpdateCategoryRequest
  ): Observable<EventCategory> {
    return this.http.put<EventCategory>(
      `${environment.apiBaseUrl}/admin/categories/${categoryId}`,
      request
    );
  }

  delete(categoryId: number): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiBaseUrl}/admin/categories/${categoryId}`
    );
  }
}
