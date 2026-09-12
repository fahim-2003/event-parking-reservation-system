import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard.service';
import { EventItem, EventService } from '../../../core/services/event.service';
import { CustomerDashboard } from '../../../core/models/dashboard.models';
import {
  CustomerNotification,
  NotificationApiService
} from '../notifications/services/notification-api.service';


@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  templateUrl: './customer-dashboard.html',
  styleUrl: './customer-dashboard.css'
})
export class CustomerDashboardComponent implements OnInit, OnDestroy {


  private readonly dashboardService =
    inject(DashboardService);

  private readonly eventService =
    inject(EventService);

  private readonly notificationApi =
    inject(NotificationApiService);

  private readonly cdr = inject(ChangeDetectorRef);

  private readonly router = inject(Router);


  dashboard: CustomerDashboard | null = null;
  upcomingEvents: EventItem[] = [];
  activeEventIndex = 0;
  featuredRotationPaused = false;

  private eventRotationTimer: number | null = null;
  notifications: CustomerNotification[] = [];
  notificationPanelOpen = false;
  notificationsLoading = false;
  searchPanelOpen = false;


  ngOnInit(): void {

    this.loadUnreadNotifications();
    this.loadUpcomingEvents();

    this.dashboardService
      .getCustomerDashboard()
      .subscribe({

        next: (response) => {

          console.log(
            'CUSTOMER DASHBOARD:',
            response
          );

          this.dashboard = response;
          this.cdr.detectChanges();

        },


        error: (error) => {

          console.error(
            'Customer dashboard failed',
            error
          );

        }

      });

  }


  toggleNotifications(): void {

    this.searchPanelOpen = false;

    this.notificationPanelOpen =
      !this.notificationPanelOpen;

    if (this.notificationPanelOpen) {
      this.loadUnreadNotifications();
    }

  }


  closeNotifications(): void {

    this.notificationPanelOpen = false;

  }


  loadUnreadNotifications(): void {

    this.notificationsLoading = true;

    this.notificationApi
      .getMine()
      .subscribe({

        next: response => {

          this.notifications =
            (response ?? [])
              .filter(notification => !notification.isRead)
              .slice(0, 5);

          this.notificationsLoading = false;
          this.cdr.detectChanges();

        },

        error: error => {

          console.error(
            'Dashboard notifications failed',
            error
          );

          this.notifications = [];
          this.notificationsLoading = false;
          this.cdr.detectChanges();

        }

      });

  }


  markNotificationRead(
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

          this.notifications =
            this.notifications.filter(
              item => item.id !== notification.id
            );

          if (this.dashboard) {

            this.dashboard = {
              ...this.dashboard,
              unreadNotifications:
                Math.max(
                  0,
                  this.dashboard.unreadNotifications - 1
                )
            };

          }

          this.cdr.detectChanges();

        },

        error: error => {

          console.error(
            'Mark notification as read failed',
            error
          );

        }

      });

  }


  formatNotificationTime(
    createdAt: string
  ): string {

    const value = new Date(createdAt);

    if (Number.isNaN(value.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat(
      'en-LK',
      {
        day: '2-digit',
        month: 'short',
        hour: 'numeric',
        minute: '2-digit'
      }
    ).format(value);

  }

  toggleSearch(): void {

    this.searchPanelOpen =
      !this.searchPanelOpen;

    if (this.searchPanelOpen) {
      this.notificationPanelOpen = false;
    }

  }


  closeSearch(): void {

    this.searchPanelOpen = false;

  }


  searchEvents(
    value: string
  ): void {

    const search = value.trim();

    if (!search) {
      return;
    }

    this.searchPanelOpen = false;

    this.router.navigate(
      ['/customer/events'],
      {
        queryParams: {
          search
        }
      }
    );

  }

  ngOnDestroy(): void {

    this.stopEventRotation();

  }


  get featuredEvent(): EventItem | null {

    return this.upcomingEvents[
      this.activeEventIndex
    ] ?? null;

  }


  nextFeaturedEvent(): void {

    if (this.upcomingEvents.length <= 1) {
      return;
    }

    this.activeEventIndex =
      (
        this.activeEventIndex + 1
      ) % this.upcomingEvents.length;

    this.restartEventRotation();

  }


  previousFeaturedEvent(): void {

    if (this.upcomingEvents.length <= 1) {
      return;
    }

    this.activeEventIndex =
      (
        this.activeEventIndex -
        1 +
        this.upcomingEvents.length
      ) % this.upcomingEvents.length;

    this.restartEventRotation();

  }


  selectFeaturedEvent(
    index: number
  ): void {

    if (
      index < 0 ||
      index >= this.upcomingEvents.length
    ) {
      return;
    }

    this.activeEventIndex = index;

    this.restartEventRotation();

  }


  pauseFeaturedRotation(): void {

    this.featuredRotationPaused = true;

  }


  resumeFeaturedRotation(): void {

    this.featuredRotationPaused = false;

  }


  getCategoryVisualClass(
    categoryName: string
  ): string {

    switch (
      categoryName
        .trim()
        .toLowerCase()
    ) {

      case 'sports':
        return 'visual-sports';

      case 'concert':
        return 'visual-concert';

      case 'cinema':
        return 'visual-cinema';

      case 'conference':
        return 'visual-conference';

      case 'festival':
        return 'visual-festival';

      default:
        return 'visual-generic';

    }

  }


  formatFeaturedMonth(
    value: string
  ): string {

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat(
      'en',
      {
        month: 'short'
      }
    )
      .format(date)
      .toUpperCase();

  }


  formatFeaturedDay(
    value: string
  ): string {

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat(
      'en',
      {
        day: '2-digit'
      }
    ).format(date);

  }


  formatFeaturedDateTime(
    value: string
  ): string {

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat(
      'en-LK',
      {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        hour: 'numeric',
        minute: '2-digit'
      }
    ).format(date);

  }


  private loadUpcomingEvents(): void {

    const now =
      new Date();

    this.eventService
      .getEvents({
        dateFromUtc:
          now.toISOString()
      })
      .subscribe({

        next: response => {

          const currentTime =
            Date.now();

          this.upcomingEvents =
            (response ?? [])
              .filter(event => {

                const start =
                  new Date(
                    event.startDateTimeUtc
                  ).getTime();

                return (
                  !Number.isNaN(start) &&
                  start >= currentTime
                );

              })
              .sort(
                (left, right) =>
                  new Date(
                    left.startDateTimeUtc
                  ).getTime() -
                  new Date(
                    right.startDateTimeUtc
                  ).getTime()
              )
              .slice(0, 8);

          this.activeEventIndex = 0;

          this.startEventRotation();

          this.cdr.detectChanges();

        },

        error: error => {

          console.error(
            'Upcoming events failed',
            error
          );

          this.upcomingEvents = [];
          this.activeEventIndex = 0;

          this.stopEventRotation();

          this.cdr.detectChanges();

        }

      });

  }


  private startEventRotation(): void {

    this.stopEventRotation();

    if (this.upcomingEvents.length <= 1) {
      return;
    }

    this.eventRotationTimer =
      window.setInterval(
        () => {

          if (
            this.featuredRotationPaused
          ) {
            return;
          }

          this.activeEventIndex =
            (
              this.activeEventIndex + 1
            ) % this.upcomingEvents.length;

          this.cdr.detectChanges();

        },
        5000
      );

  }


  private stopEventRotation(): void {

    if (
      this.eventRotationTimer === null
    ) {
      return;
    }

    window.clearInterval(
      this.eventRotationTimer
    );

    this.eventRotationTimer = null;

  }


  private restartEventRotation(): void {

    this.stopEventRotation();
    this.startEventRotation();

  }

  bookFeaturedEvent(
    eventId: number
  ): void {

    this.router.navigate([
      '/customer/bookings/create',
      eventId
    ]);

  }
}



