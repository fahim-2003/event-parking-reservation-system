import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';

import {
  EventItem,
  EventService
} from '../../../../../core/services/event.service';

import {
  SeatLabelPipe
} from '../../../../../shared/pipes/seat-label.pipe';

import {
  SlotCodePipe
} from '../../../../../shared/pipes/slot-code.pipe';

import {
  BookingService,
  ParkingSlotResponse,
  SeatResponse
} from '../../services/booking.service';

import {
  SeatSelector
} from '../../components/seat-selector/seat-selector';

import {
  ParkingSelector
} from '../../components/parking-selector/parking-selector';

@Component({
  selector: 'app-booking-create',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    SeatSelector,
    ParkingSelector,
    SeatLabelPipe,
    SlotCodePipe
  ],
  templateUrl: './booking-create.html',
  styleUrl: './booking-create.css'
})
export class BookingCreate implements OnInit {

  private readonly route =
    inject(ActivatedRoute);

  private readonly router =
    inject(Router);

  private readonly bookingService =
    inject(BookingService);

  private readonly eventService =
    inject(EventService);

  eventId = 0;

  event:
    EventItem | null = null;

  eventLoading = true;

  seats: SeatResponse[] = [];

  parkingSlots:
    ParkingSlotResponse[] = [];

  selectedSeatIds:
    number[] = [];

  selectedParkingSlotId:
    number | null = null;

  loadingResources = false;
  loading = false;

  private seatsFinished = false;
  private parkingFinished = false;

  message = '';
  resourceError = '';
  eventError = '';

  ngOnInit(): void {

    this.eventId =
      Number(
        this.route
          .snapshot
          .paramMap
          .get('eventId')
      );

    if (!this.eventId) {

      this.resourceError =
        'Invalid event.';

      this.eventLoading = false;

      return;
    }

    this.loadEventDetails();
    this.loadAvailability();
  }

  loadEventDetails(): void {

    this.eventLoading = true;
    this.eventError = '';

    this.eventService
      .getEvents()
      .pipe(
        finalize(() => {
          this.eventLoading = false;
        })
      )
      .subscribe({

        next: events => {

          this.event =
            events.find(
              item =>
                item.id === this.eventId
            ) ?? null;

          if (!this.event) {

            this.eventError =
              'Event details are unavailable.';
          }
        },

        error: error => {

          console.error(
            'Event details failed',
            error
          );

          this.eventError =
            'Unable to load event details.';
        }
      });
  }

  loadAvailability(): void {

    this.loadingResources = true;
    this.resourceError = '';

    this.seatsFinished = false;
    this.parkingFinished = false;

    this.bookingService
      .getSeats(this.eventId)
      .pipe(
        finalize(() => {
          this.seatsFinished = true;
          this.finishAvailabilityLoading();
        })
      )
      .subscribe({

        next: response => {

          this.seats =
            response ?? [];

          const availableIds =
            new Set(
              this.seats
                .filter(
                  seat =>
                    seat.status ===
                    'Available'
                )
                .map(
                  seat => seat.id
                )
            );

          this.selectedSeatIds =
            this.selectedSeatIds
              .filter(
                id =>
                  availableIds.has(id)
              );
        },

        error: error => {

          console.error(
            'Seat availability failed',
            error
          );

          this.resourceError =
            'Unable to load seat availability.';
        }
      });

    this.bookingService
      .getParkingSlots(this.eventId)
      .pipe(
        finalize(() => {
          this.parkingFinished = true;
          this.finishAvailabilityLoading();
        })
      )
      .subscribe({

        next: response => {

          this.parkingSlots =
            response ?? [];

          if (
            this.selectedParkingSlotId
          ) {

            const selectedParking =
              this.parkingSlots.find(
                slot =>
                  slot.id ===
                  this.selectedParkingSlotId
              );

            if (
              !selectedParking ||
              selectedParking.status !==
                'Available'
            ) {

              this.selectedParkingSlotId =
                null;
            }
          }
        },

        error: error => {

          console.error(
            'Parking availability failed',
            error
          );

          this.resourceError =
            this.resourceError ||
            'Unable to load parking availability.';
        }
      });
  }

  private finishAvailabilityLoading():
    void {

    if (
      this.seatsFinished &&
      this.parkingFinished
    ) {

      this.loadingResources = false;
    }
  }

  toggleSeat(
    seat: SeatResponse
  ): void {

    if (
      seat.status !== 'Available' ||
      this.loading
    ) {
      return;
    }

    if (
      this.isSeatSelected(seat.id)
    ) {

      this.selectedSeatIds =
        this.selectedSeatIds.filter(
          id => id !== seat.id
        );

    } else {

      this.selectedSeatIds = [
        ...this.selectedSeatIds,
        seat.id
      ];
    }

    this.message = '';
  }

  isSeatSelected(
    seatId: number
  ): boolean {

    return this.selectedSeatIds
      .includes(seatId);
  }

  selectParking(
    slot: ParkingSlotResponse
  ): void {

    if (
      slot.status !== 'Available' ||
      this.loading
    ) {
      return;
    }

    this.selectedParkingSlotId =
      this.selectedParkingSlotId ===
      slot.id
        ? null
        : slot.id;

    this.message = '';
  }

  clearParking(): void {

    this.selectedParkingSlotId =
      null;
  }

  createBooking(): void {

    if (
      this.selectedSeatIds.length === 0
    ) {

      this.message =
        'Please select at least one available seat.';

      return;
    }

    this.loading = true;
    this.message = '';

    this.bookingService
      .create({

        eventId:
          this.eventId,

        seatIds:
          [...this.selectedSeatIds],

        parkingSlotId:
          this.selectedParkingSlotId
      })
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({

        next: response => {

          this.router.navigate(
            [
              '/customer/bookings/summary'
            ],
            {
              state: {
                booking: response
              }
            }
          );
        },

        error: error => {

          console.error(
            'Booking failed',
            error
          );

          this.message =
            error.error?.detail ??
            error.error?.title ??
            'Booking failed.';

          if (
            error.status === 409
          ) {

            this.loadAvailability();
          }
        }
      });
  }

  get selectedSeats():
    SeatResponse[] {

    return this.seats.filter(
      seat =>
        this.selectedSeatIds
          .includes(seat.id)
    );
  }

  get selectedParkingSlot():
    ParkingSlotResponse | undefined {

    if (
      !this.selectedParkingSlotId
    ) {
      return undefined;
    }

    return this.parkingSlots.find(
      slot =>
        slot.id ===
        this.selectedParkingSlotId
    );
  }

  get availableSeatCount():
    number {

    return this.seats.filter(
      seat =>
        seat.status === 'Available'
    ).length;
  }

  get availableParkingCount():
    number {

    return this.parkingSlots.filter(
      slot =>
        slot.status === 'Available'
    ).length;
  }

  get ticketSubtotal():
    number {

    return (
      (this.event?.ticketPrice ?? 0) *
      this.selectedSeatIds.length
    );
  }

  get parkingSubtotal():
    number {

    if (
      !this.selectedParkingSlotId
    ) {
      return 0;
    }

    return (
      this.event?.parkingFee ?? 0
    );
  }

  get estimatedTotal():
    number {

    return (
      this.ticketSubtotal +
      this.parkingSubtotal
    );
  }
}