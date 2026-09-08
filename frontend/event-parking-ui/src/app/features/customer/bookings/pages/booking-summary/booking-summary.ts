import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  EventItem,
  EventService
} from '../../../../../core/services/event.service';
import { BookingStatusPipe } from '../../../../../shared/pipes/booking-status.pipe';
import { BookingResponse } from '../../models/booking.models';
import { BookingService } from '../../services/booking.service';

@Component({
  selector: 'app-booking-summary',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    BookingStatusPipe
  ],
  templateUrl: './booking-summary.html',
  styleUrl: './booking-summary.css'
})
export class BookingSummary implements OnInit {

  private readonly bookingService =
    inject(BookingService);

  private readonly eventService =
    inject(EventService);

  booking: BookingResponse | null =
    history.state?.booking ?? null;

  bookings: BookingResponse[] = [];

  eventsById =
    new Map<number, EventItem>();

  loading = true;

  cancellingBookingId:
    number | null = null;

  errorMessage = '';
  actionMessage = '';

  ngOnInit(): void {
    this.loadEvents();
    this.loadBookings();
  }

  loadEvents(): void {

    this.eventService
      .getEvents()
      .subscribe({
        next: events => {

          this.eventsById =
            new Map(
              events.map(
                event => [
                  event.id,
                  event
                ]
              )
            );
        },

        error: error => {
          console.error(
            'Unable to load event details for bookings',
            error
          );
        }
      });
  }

  loadBookings(): void {

    this.loading = true;
    this.errorMessage = '';

    this.bookingService
      .getMine()
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: response => {

          this.bookings = response;

          if (this.booking) {

            const refreshed =
              response.find(
                item =>
                  item.id === this.booking?.id
              );

            if (refreshed) {
              this.booking = refreshed;
            }
          }
        },

        error: error => {

          console.error(error);

          this.errorMessage =
            'Unable to load bookings.';
        }
      });
  }

  eventFor(
    eventId: number
  ): EventItem | undefined {

    return this.eventsById.get(eventId);
  }

  eventName(
    eventId: number
  ): string {

    return (
      this.eventFor(eventId)?.name ??
      'Event Reservation'
    );
  }

  eventVenue(
    eventId: number
  ): string {

    return (
      this.eventFor(eventId)?.venueName ??
      'Venue information unavailable'
    );
  }

  eventCategory(
    eventId: number
  ): string {

    return (
      this.eventFor(eventId)?.categoryName ??
      'Event'
    );
  }

  get confirmedCount(): number {

    return this.bookings.filter(
      item => item.status === 'Confirmed'
    ).length;
  }

  get heldCount(): number {

    return this.bookings.filter(
      item => item.status === 'Held'
    ).length;
  }

  get cancelledCount(): number {

    return this.bookings.filter(
      item => item.status === 'Cancelled'
    ).length;
  }

  canCancel(
    booking: BookingResponse
  ): boolean {

    return (
      booking.status === 'Held' ||
      booking.status === 'Confirmed'
    );
  }

  cancelBooking(
    booking: BookingResponse
  ): void {

    if (!this.canCancel(booking)) {
      return;
    }

    const confirmed =
      window.confirm(
        `Cancel booking ${booking.bookingNumber}?`
      );

    if (!confirmed) {
      return;
    }

    this.cancellingBookingId =
      booking.id;

    this.errorMessage = '';
    this.actionMessage = '';

    this.bookingService
      .cancel(booking.id)
      .pipe(
        finalize(() => {
          this.cancellingBookingId =
            null;
        })
      )
      .subscribe({
        next: cancelled => {

          this.actionMessage =
            `${this.eventName(cancelled.eventId)} reservation cancelled successfully.`;

          this.bookings =
            this.bookings.map(
              item =>
                item.id === cancelled.id
                  ? cancelled
                  : item
            );

          if (
            this.booking?.id ===
            cancelled.id
          ) {
            this.booking =
              cancelled;
          }
        },

        error: error => {

          console.error(error);

          this.errorMessage =
            error.error?.detail ??
            'Unable to cancel booking.';
        }
      });
  }
}