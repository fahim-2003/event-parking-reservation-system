import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';


export interface EventItem {

  id: number;

  name: string;

  date: string;

  venueId: number;

  categoryId: number;

  capacity: number;

}



@Injectable({
  providedIn: 'root'
})
export class EventService {


  private http = inject(HttpClient);


  private apiUrl =
    `\/events`;



  getEvents(): Observable<EventItem[]> {

    return this.http.get<EventItem[]>(
      this.apiUrl
    );

  }



  getEventById(id:number): Observable<EventItem> {

    return this.http.get<EventItem>(
      `${this.apiUrl}/${id}`
    );

  }


}
