import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';

import {
  CustomerNotification,
  NotificationApiService
} from '../../services/notification-api.service';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule],
  styleUrl: './notification-list.css',
  templateUrl: './notification-list.html'
})
export class NotificationList implements OnInit {

  private readonly notificationApi =
    inject(NotificationApiService);

  notifications: CustomerNotification[] = [];

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {

    this.loading = true;
    this.errorMessage = '';

    this.notificationApi
      .getMine()
      .subscribe({
        next: response => {

          this.notifications =
            response ?? [];

          this.loading = false;
        },

        error: error => {

          console.error(error);

          this.errorMessage =
            'Unable to load notifications.';

          this.loading = false;
        }
      });
  }

  markRead(
    notification: CustomerNotification
  ): void {

    if (notification.isRead) {
      return;
    }

    this.notificationApi
      .markRead(notification.id)
      .subscribe({
        next: () => {

          notification.isRead = true;
        },

        error: error => {

          console.error(error);
        }
      });
  }

  get unreadCount(): number {

    return this.notifications.filter(
      notification =>
        !notification.isRead
    ).length;
  }

  notificationKind(
    notification: CustomerNotification
  ):
    | 'payment'
    | 'cancelled'
    | 'booking'
    | 'general' {

    const message =
      notification.message
        .toLowerCase();

    if (
      message.includes('payment') &&
      message.includes('completed')
    ) {
      return 'payment';
    }

    if (
      message.includes('cancelled') ||
      message.includes('canceled')
    ) {
      return 'cancelled';
    }

    if (
      message.includes('booking') ||
      message.includes('held') ||
      message.includes('created')
    ) {
      return 'booking';
    }

    return 'general';
  }

  notificationTitle(
    notification: CustomerNotification
  ): string {

    switch (
      this.notificationKind(notification)
    ) {

      case 'payment':
        return 'Payment Completed';

      case 'cancelled':
        return 'Booking Cancelled';

      case 'booking':
        return 'Booking Update';

      default:
        return 'Notification';
    }
  }

  notificationLetter(
    notification: CustomerNotification
  ): string {

    switch (
      this.notificationKind(notification)
    ) {

      case 'payment':
        return '$';

      case 'cancelled':
        return 'X';

      case 'booking':
        return 'B';

      default:
        return 'N';
    }
  }
}