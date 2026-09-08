import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';
import { EventItem } from '../../../events/models/event.model';
import { EventApiService } from '../../../events/services/event-api.service';
import { Seat } from '../../models/seat.model';
import { SeatApiService } from '../../services/seat-api.service';

@Component({
  selector: 'app-seat-layout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './seat-layout.html',
  styleUrl: './seat-layout.css'
})
export class SeatLayout {
  private readonly eventApi = inject(EventApiService);
  private readonly seatApi = inject(SeatApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly events = signal<EventItem[]>([]);
  readonly seats = signal<Seat[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingSeatId = signal<number | null>(null);

  readonly layoutForm = this.formBuilder.nonNullable.group({
    eventId: [0, [Validators.required, Validators.min(1)]],
    rows: [1, [Validators.required, Validators.min(1)]],
    seatsPerRow: [1, [Validators.required, Validators.min(1)]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    rowLabel: ['', Validators.required],
    seatNumber: [1, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    this.loadEvents();
  }

  get selectedEvent(): EventItem | undefined {
    return this.events().find(
      eventItem =>
        eventItem.id === this.layoutForm.controls.eventId.value
    );
  }

  get requestedSeatCount(): number {
    return (
      this.layoutForm.controls.rows.value *
      this.layoutForm.controls.seatsPerRow.value
    );
  }
  get availableSeatCount(): number {
    return this.seats().filter(
      seat => seat.status === 'Available'
    ).length;
  }

  get heldSeatCount(): number {
    return this.seats().filter(
      seat => seat.status === 'Held'
    ).length;
  }

  get bookedSeatCount(): number {
    return this.seats().filter(
      seat => seat.status === 'Booked'
    ).length;
  }

  get layoutMatchesCapacity(): boolean {
    return !!this.selectedEvent &&
      this.requestedSeatCount === this.selectedEvent.capacity;
  }

  loadEvents(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.eventApi.getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: events => this.events.set(events),
        error: () =>
          this.errorMessage.set(
            'Unable to load events. Please try again.'
          )
      });
  }

  eventChanged(): void {
    this.cancelEdit();
    this.seats.set([]);
    this.errorMessage.set('');

    const eventId = this.layoutForm.controls.eventId.value;

    if (eventId <= 0) {
      return;
    }

    this.loadSeats(eventId);
  }

  loadSeats(eventId: number): void {
    this.loading.set(true);

    this.seatApi.getByEvent(eventId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: seats => {
          this.seats.set(seats);

          if (seats.length > 0) {
            const rowLabels = [
              ...new Set(
                seats.map(seat => seat.rowLabel)
              )
            ];

            const seatsPerRow =
              rowLabels.length === 0
                ? 1
                : Math.max(
                    ...rowLabels.map(
                      rowLabel =>
                        seats.filter(
                          seat =>
                            seat.rowLabel === rowLabel
                        ).length
                    )
                  );

            this.layoutForm.patchValue(
              {
                rows: Math.max(
                  rowLabels.length,
                  1
                ),
                seatsPerRow: Math.max(
                  seatsPerRow,
                  1
                )
              },
              {
                emitEvent: false
              }
            );
          }
        },
        error: () =>
          this.errorMessage.set(
            'Unable to load the seat layout.'
          )
      });
  }

  generateLayout(): void {
    if (this.layoutForm.invalid) {
      this.layoutForm.markAllAsTouched();
      return;
    }

    const eventItem = this.selectedEvent;

    if (!eventItem) {
      this.errorMessage.set('Please select an event.');
      return;
    }

    const value = this.layoutForm.getRawValue();

    if (this.requestedSeatCount !== eventItem.capacity) {
      this.errorMessage.set(
        `Seat layout must contain exactly ${eventItem.capacity} seats. ` +
        `Current layout contains ${this.requestedSeatCount}.`
      );
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    this.seatApi.generate(eventItem.id, {
      rows: value.rows,
      seatsPerRow: value.seatsPerRow
    })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: seats => {
          this.seats.set(seats);

          if (seats.length > 0) {
            const rowLabels = [
              ...new Set(
                seats.map(seat => seat.rowLabel)
              )
            ];

            const seatsPerRow =
              rowLabels.length === 0
                ? 1
                : Math.max(
                    ...rowLabels.map(
                      rowLabel =>
                        seats.filter(
                          seat =>
                            seat.rowLabel === rowLabel
                        ).length
                    )
                  );

            this.layoutForm.patchValue(
              {
                rows: Math.max(
                  rowLabels.length,
                  1
                ),
                seatsPerRow: Math.max(
                  seatsPerRow,
                  1
                )
              },
              {
                emitEvent: false
              }
            );
          }
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to generate seat layout.';

          this.errorMessage.set(detail);
        }
      });
  }

  editSeat(seat: Seat): void {
    if (seat.status !== 'Available') {
      return;
    }

    this.editingSeatId.set(seat.id);

    this.editForm.setValue({
      rowLabel: seat.rowLabel,
      seatNumber: seat.seatNumber
    });
  }

  saveSeat(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const eventItem = this.selectedEvent;
    const seatId = this.editingSeatId();

    if (!eventItem || seatId === null) {
      return;
    }

    const currentSeat = this.seats().find(
      seat => seat.id === seatId
    );

    if (!currentSeat) {
      return;
    }

    const value = this.editForm.getRawValue();
    const rowLabel = value.rowLabel.trim().toUpperCase();

    if (!rowLabel) {
      this.editForm.controls.rowLabel.setErrors({
        required: true
      });
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    this.seatApi.update(
      eventItem.id,
      seatId,
      {
        rowLabel,
        seatNumber: value.seatNumber,
        rowVersion: currentSeat.rowVersion
      }
    )
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: updatedSeat => {
          this.seats.update(seats =>
            seats.map(seat =>
              seat.id === updatedSeat.id
                ? updatedSeat
                : seat
            )
          );

          this.cancelEdit();
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to update seat. Refresh and try again.';

          this.errorMessage.set(detail);
        }
      });
  }


  deleteSeat(seat: Seat): void {
    if (seat.status !== 'Available') {
      return;
    }

    const eventItem = this.selectedEvent;

    if (!eventItem) {
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    this.seatApi.delete(
      eventItem.id,
      seat.id,
      seat.rowVersion
    )
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.seats.update(seats =>
            seats.filter(item => item.id !== seat.id)
          );
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to delete seat. Refresh and try again.';

          this.errorMessage.set(detail);
        }
      });
  }
  cancelEdit(): void {
    this.editingSeatId.set(null);

    this.editForm.reset({
      rowLabel: '',
      seatNumber: 1
    });
  }
}
