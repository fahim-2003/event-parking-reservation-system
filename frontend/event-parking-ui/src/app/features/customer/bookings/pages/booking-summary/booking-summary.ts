import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BookingResponse } from '../../models/booking.models';
import { BookingService } from '../../services/booking.service';
import { BookingStatusPipe } from '../../../../../shared/pipes/booking-status.pipe';

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

  booking: BookingResponse | null =
    history.state?.booking ?? null;

  bookings: BookingResponse[] = [];

  loading = true;
  cancellingBookingId: number | null = null;

  errorMessage = '';
  actionMessage = '';

  ngOnInit(): void {
    this.loadBookings();
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

    this.cancellingBookingId = booking.id;
    this.errorMessage = '';
    this.actionMessage = '';

    this.bookingService
      .cancel(booking.id)
      .pipe(
        finalize(() => {
          this.cancellingBookingId = null;
        })
      )
      .subscribe({
        next: cancelled => {

          this.actionMessage =
            `Booking ${cancelled.bookingNumber} cancelled successfully.`;

          this.bookings =
            this.bookings.map(
              item =>
                item.id === cancelled.id
                  ? cancelled
                  : item
            );

          if (
            this.booking?.id === cancelled.id
          ) {
            this.booking = cancelled;
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
