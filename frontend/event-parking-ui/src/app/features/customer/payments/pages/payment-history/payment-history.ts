import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import {
  BookingService
} from '../../../bookings/services/booking.service';
import {
  PaymentApiService,
  PaymentResponse
} from '../../services/payment-api.service';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './payment-history.css',
  templateUrl: './payment-history.html'
})
export class PaymentHistory implements OnInit {

  private readonly route = inject(ActivatedRoute);
  private readonly paymentApi = inject(PaymentApiService);
  private readonly bookingService = inject(BookingService);

  bookingId = 0;
  bookingNumber = '';
  paymentMethod = 'Card';

  cardholderName = '';
  cardNumber = '';
  expiryDate = '';
  cvv = '';

  mobileNumber = '';

  loading = false;
  errorMessage = '';
  successMessage = '';

  payments: PaymentResponse[] = [];

  ngOnInit(): void {
    this.bookingId =
      Number(
        this.route.snapshot.queryParamMap.get('bookingId')
      ) || 0;

    if (this.bookingId > 0) {
      this.loadBookingReference();
    }

    this.loadPayments();
  }

  selectMethod(method: string): void {
    this.paymentMethod = method;
    this.errorMessage = '';
  }

  pay(): void {

    if (!this.bookingId) {
      this.errorMessage =
        'No booking selected for payment.';
      return;
    }

    if (this.paymentMethod === 'Card') {
      if (
        !this.cardholderName.trim() ||
        !this.cardNumber.trim() ||
        !this.expiryDate.trim() ||
        !this.cvv.trim()
      ) {
        this.errorMessage =
          'Please complete the card details.';
        return;
      }
    }

    if (
      this.paymentMethod === 'Mobile' &&
      !this.mobileNumber.trim()
    ) {
      this.errorMessage =
        'Please enter the mobile payment number.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.paymentApi
      .process(
        this.bookingId,
        this.paymentMethod
      )
      .subscribe({
        next: payment => {
          this.successMessage =
            `Payment completed successfully. Amount: Rs. ${payment.amount.toFixed(2)}`;

          this.loading = false;
          this.bookingId = 0;

          this.cardholderName = '';
          this.cardNumber = '';
          this.expiryDate = '';
          this.cvv = '';
          this.mobileNumber = '';

          if (this.bookingId > 0) {
      this.loadBookingReference();
    }

    this.loadPayments();
        },

        error: error => {
          console.error(error);

          this.errorMessage =
            error.error?.detail ??
            'Payment failed.';

          this.loading = false;
        }
      });
  }

  private loadBookingReference(): void {

    this.bookingService
      .getMine()
      .subscribe({

        next: bookings => {

          const booking =
            (bookings ?? []).find(
              item =>
                item.id === this.bookingId
            );

          this.bookingNumber =
            booking?.bookingNumber ?? '';

        },

        error: error => {
          console.error(
            'Booking reference failed',
            error
          );

          this.bookingNumber = '';
        }

      });

  }
  loadPayments(): void {
    this.paymentApi
      .getMine()
      .subscribe({
        next: payments => {
          this.payments = payments;
        },

        error: error => {
          console.error(error);
        }
      });
  }
}