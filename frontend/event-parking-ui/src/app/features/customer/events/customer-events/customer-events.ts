import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService, EventItem } from '../../../../core/services/event.service';

@Component({
  selector: 'app-customer-events',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './customer-events.html',
  styleUrl: './customer-events.css'
})
export class CustomerEventsComponent implements OnInit {


  private eventService = inject(EventService);


  events: EventItem[] = [];

  loading = true;

  errorMessage = '';



  ngOnInit(): void {

    this.loadEvents();

  }



  loadEvents(): void {

    this.eventService.getEvents()
      .subscribe({

        next: (response) => {

          this.events = response;

          this.loading = false;

        },


        error: (error) => {

          console.error(
            'Event loading failed',
            error
          );

          this.errorMessage =
            'Unable to load events';

          this.loading = false;

        }

      });

  }



}
