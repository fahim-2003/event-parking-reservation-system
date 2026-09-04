import { Component, inject } from '@angular/core';
import { BookingService } from '../../services/booking.service';
import {
  BookingResponse,
  CreateBookingRequest
} from '../../models/booking.models';

@Component({
  selector: 'app-booking-summary',
  imports: [],
  templateUrl: './booking-summary.html',
  styleUrl: './booking-summary.css'
})
export class BookingSummary {

  private readonly bookingService = inject(BookingService);

  selectedBooking: CreateBookingRequest = {
    eventId: 1,
    seatId: null,
    parkingSlotId: null
  };

  bookingResult: BookingResponse | null = null;

  errorMessage = '';

  createBooking(): void {
    this.bookingService
      .create(this.selectedBooking)
      .subscribe({
        next: response => {
          this.bookingResult = response;
          this.errorMessage = '';
        },
        error: error => {
          this.errorMessage =
            error.error?.detail ??
            'Booking failed.';
        }
      });
  }
}