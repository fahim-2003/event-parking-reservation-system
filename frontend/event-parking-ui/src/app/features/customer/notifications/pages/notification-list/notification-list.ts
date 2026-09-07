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

    this.notificationApi
      .getMine()
      .subscribe({
        next: response => {
          this.notifications = response;
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

  markRead(notification: CustomerNotification): void {

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
}
