import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  BookingService,
  ParkingSlotResponse,
  SeatResponse
} from '../../services/booking.service';
import { SeatSelector } from '../../components/seat-selector/seat-selector';
import { ParkingSelector } from '../../components/parking-selector/parking-selector';

@Component({
  selector: 'app-booking-create',
  standalone: true,
  imports: [
    CommonModule,
    SeatSelector,
    ParkingSelector
  ],
  templateUrl: './booking-create.html',
  styleUrl: './booking-create.css'
})
export class BookingCreate implements OnInit {

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingService = inject(BookingService);

  eventId = 0;

  seats: SeatResponse[] = [];
  parkingSlots: ParkingSlotResponse[] = [];

  selectedSeatIds: number[] = [];
  selectedParkingSlotId: number | null = null;

  loadingResources = false;
  loading = false;

  private seatsFinished = false;
  private parkingFinished = false;

  message = '';
  resourceError = '';

  ngOnInit(): void {
    this.eventId =
      Number(
        this.route.snapshot.paramMap.get('eventId')
      );

    if (!this.eventId) {
      this.resourceError = 'Invalid event.';
      return;
    }

    this.loadAvailability();
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
          this.seats = response ?? [];

          const availableIds =
            new Set(
              this.seats
                .filter(
                  seat =>
                    seat.status === 'Available'
                )
                .map(seat => seat.id)
            );

          this.selectedSeatIds =
            this.selectedSeatIds.filter(
              id => availableIds.has(id)
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
          this.parkingSlots = response ?? [];

          if (this.selectedParkingSlotId) {
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
              this.selectedParkingSlotId = null;
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

  private finishAvailabilityLoading(): void {
    if (
      this.seatsFinished &&
      this.parkingFinished
    ) {
      this.loadingResources = false;
    }
  }

  toggleSeat(seat: SeatResponse): void {
    if (
      seat.status !== 'Available' ||
      this.loading
    ) {
      return;
    }

    if (this.isSeatSelected(seat.id)) {
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

  isSeatSelected(seatId: number): boolean {
    return this.selectedSeatIds.includes(seatId);
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
      this.selectedParkingSlotId === slot.id
        ? null
        : slot.id;

    this.message = '';
  }

  clearParking(): void {
    this.selectedParkingSlotId = null;
  }

  createBooking(): void {
    if (this.selectedSeatIds.length === 0) {
      this.message =
        'Please select at least one available seat.';
      return;
    }

    this.loading = true;
    this.message = '';

    this.bookingService
      .create({
        eventId: this.eventId,
        seatIds: [...this.selectedSeatIds],
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
            ['/customer/bookings/summary'],
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

          if (error.status === 409) {
            this.loadAvailability();
          }
        }
      });
  }

  get availableSeatCount(): number {
    return this.seats.filter(
      seat => seat.status === 'Available'
    ).length;
  }

  get availableParkingCount(): number {
    return this.parkingSlots.filter(
      slot => slot.status === 'Available'
    ).length;
  }
}
