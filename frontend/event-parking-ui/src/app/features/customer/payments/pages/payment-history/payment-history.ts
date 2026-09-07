import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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

  bookingId = 0;
  paymentMethod = 'Card';

  loading = false;
  errorMessage = '';
  successMessage = '';

  payments: PaymentResponse[] = [];

  ngOnInit(): void {
    this.bookingId =
      Number(
        this.route.snapshot.queryParamMap.get('bookingId')
      ) || 0;

    this.loadPayments();
  }

  pay(): void {

    if (!this.bookingId) {
      this.errorMessage =
        'No booking selected for payment.';
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
            `Payment completed. Amount: ${payment.amount.toFixed(2)}`;

          this.loading = false;
          this.bookingId = 0;

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
