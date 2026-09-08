import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';
import { ConfirmationDialog } from '../../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { EventItem } from '../../../events/models/event.model';
import { EventApiService } from '../../../events/services/event-api.service';
import { ParkingSlot } from '../../models/parking-slot.model';
import { ParkingApiService } from '../../services/parking-api.service';

@Component({
  selector: 'app-parking-layout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ConfirmationDialog
  ],
  templateUrl: './parking-layout.html',
  styleUrl: './parking-layout.css'
})
export class ParkingLayout {
  private readonly eventApi = inject(EventApiService);
  private readonly parkingApi = inject(ParkingApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly events = signal<EventItem[]>([]);
  readonly parkingSlots = signal<ParkingSlot[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingSlotId = signal<number | null>(null);
  readonly pendingDeleteSlot = signal<ParkingSlot | null>(null);

  readonly layoutForm = this.formBuilder.nonNullable.group({
    eventId: [0, [Validators.required, Validators.min(1)]],
    zone: ['', Validators.required],
    numberOfSlots: [1, [Validators.required, Validators.min(1)]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    zone: ['', Validators.required],
    slotNumber: [1, [Validators.required, Validators.min(1)]]
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

  get availableSlotCount(): number {
    return this.parkingSlots().filter(
      slot => slot.status === 'Available'
    ).length;
  }

  get heldSlotCount(): number {
    return this.parkingSlots().filter(
      slot => slot.status === 'Held'
    ).length;
  }

  get occupiedSlotCount(): number {
    return this.parkingSlots().filter(
      slot => slot.status === 'Occupied'
    ).length;
  }

  get zoneCount(): number {
    return new Set(
      this.parkingSlots().map(slot => slot.zone)
    ).size;
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
    this.pendingDeleteSlot.set(null);
    this.parkingSlots.set([]);
    this.errorMessage.set('');

    const eventId = this.layoutForm.controls.eventId.value;

    if (eventId <= 0) {
      return;
    }

    this.loadParkingSlots(eventId);
  }

  loadParkingSlots(eventId: number): void {
    this.loading.set(true);

    this.parkingApi.getByEvent(eventId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: slots => this.parkingSlots.set(slots),
        error: () =>
          this.errorMessage.set(
            'Unable to load the parking layout.'
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
    const zone = value.zone.trim().toUpperCase();

    if (!zone) {
      this.layoutForm.controls.zone.setErrors({
        required: true
      });
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    this.parkingApi.generate(
      eventItem.id,
      {
        zone,
        numberOfSlots: value.numberOfSlots
      }
    )
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.layoutForm.controls.zone.setValue('');
          this.layoutForm.controls.numberOfSlots.setValue(1);
          this.loadParkingSlots(eventItem.id);
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to generate parking layout.';

          this.errorMessage.set(detail);
        }
      });
  }

  editSlot(slot: ParkingSlot): void {
    if (slot.status !== 'Available') {
      return;
    }

    this.editingSlotId.set(slot.id);
    this.errorMessage.set('');

    this.editForm.setValue({
      zone: slot.zone,
      slotNumber: slot.slotNumber
    });
  }

  saveSlot(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const eventItem = this.selectedEvent;
    const slotId = this.editingSlotId();

    if (!eventItem || slotId === null) {
      return;
    }

    const currentSlot = this.parkingSlots().find(
      slot => slot.id === slotId
    );

    if (!currentSlot) {
      return;
    }

    const value = this.editForm.getRawValue();
    const zone = value.zone.trim().toUpperCase();

    if (!zone) {
      this.editForm.controls.zone.setErrors({
        required: true
      });
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    this.parkingApi.update(
      eventItem.id,
      slotId,
      {
        zone,
        slotNumber: value.slotNumber,
        rowVersion: currentSlot.rowVersion
      }
    )
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: updatedSlot => {
          this.parkingSlots.update(slots =>
            slots
              .map(slot =>
                slot.id === updatedSlot.id
                  ? updatedSlot
                  : slot
              )
              .sort((left, right) =>
                left.zone.localeCompare(right.zone) ||
                left.slotNumber - right.slotNumber
              )
          );

          this.cancelEdit();
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to update parking slot. Refresh and try again.';

          this.errorMessage.set(detail);
        }
      });
  }

  cancelEdit(): void {
    this.editingSlotId.set(null);

    this.editForm.reset({
      zone: '',
      slotNumber: 1
    });
  }

  deleteSlot(slot: ParkingSlot): void {
    if (slot.status !== 'Available') {
      return;
    }

    this.pendingDeleteSlot.set(slot);
  }

  cancelDelete(): void {
    this.pendingDeleteSlot.set(null);
  }

  confirmDelete(): void {
    const eventItem = this.selectedEvent;
    const slot = this.pendingDeleteSlot();

    if (!eventItem || !slot) {
      return;
    }

    this.errorMessage.set('');

    this.parkingApi.delete(
      eventItem.id,
      slot.id,
      slot.rowVersion
    ).subscribe({
      next: () => {
        this.pendingDeleteSlot.set(null);
        this.loadParkingSlots(eventItem.id);
      },
      error: error => {
        this.pendingDeleteSlot.set(null);

        const detail =
          error?.error?.detail ??
          'Unable to delete parking slot.';

        this.errorMessage.set(detail);
      }
    });
  }
}
