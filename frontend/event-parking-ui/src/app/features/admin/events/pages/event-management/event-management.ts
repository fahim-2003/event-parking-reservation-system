import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { ConfirmationDialog } from '../../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { ShortTextPipe } from '../../../../../shared/pipes/short-text.pipe';
import { EventCategory } from '../../../categories/models/category.model';
import { CategoryApiService } from '../../../categories/services/category-api.service';
import { Venue } from '../../../venues/models/venue.model';
import { VenueApiService } from '../../../venues/services/venue-api.service';
import {
  CreateEventRequest,
  EventItem
} from '../../models/event.model';
import { EventApiService } from '../../services/event-api.service';

@Component({
  selector: 'app-event-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ConfirmationDialog,
    ShortTextPipe
  ],
  templateUrl: './event-management.html',
  styleUrl: './event-management.css'
})
export class EventManagement {
  private readonly eventApi = inject(EventApiService);
  private readonly venueApi = inject(VenueApiService);
  private readonly categoryApi = inject(CategoryApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly events = signal<EventItem[]>([]);
  readonly venues = signal<Venue[]>([]);
  readonly categories = signal<EventCategory[]>([]);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingEventId = signal<number | null>(null);
  readonly pendingDeleteEvent = signal<EventItem | null>(null);

  readonly eventForm = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    venueId: [0, [Validators.required, Validators.min(1)]],
    eventCategoryId: [0, [Validators.required, Validators.min(1)]],
    startDateTime: ['', Validators.required],
    endDateTime: ['', Validators.required],
    ticketPrice: [0, [Validators.required, Validators.min(0)]],
    parkingFee: [0, [Validators.required, Validators.min(0)]],
    capacity: [1, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    this.loadPageData();
  }

  loadPageData(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    forkJoin({
      events: this.eventApi.getAll(),
      venues: this.venueApi.getAll(),
      categories: this.categoryApi.getAll()
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: result => {
          this.events.set(result.events);
          this.venues.set(result.venues);
          this.categories.set(result.categories);
        },
        error: () =>
          this.errorMessage.set(
            'Unable to load event management data. Please try again.'
          )
      });
  }

  submit(): void {
    if (this.eventForm.invalid) {
      this.eventForm.markAllAsTouched();
      return;
    }

    const formValue = this.eventForm.getRawValue();

    const name = formValue.name.trim();

    if (!name) {
      this.eventForm.controls.name.setErrors({ required: true });
      return;
    }

    const start = new Date(formValue.startDateTime);
    const end = new Date(formValue.endDateTime);

    if (
      Number.isNaN(start.getTime()) ||
      Number.isNaN(end.getTime()) ||
      end <= start
    ) {
      this.errorMessage.set(
        'Event end date/time must be after the start date/time.'
      );
      return;
    }

    const selectedVenue = this.venues().find(
      venue => venue.id === formValue.venueId
    );

    if (!selectedVenue) {
      this.errorMessage.set('Please select a valid venue.');
      return;
    }

    if (formValue.capacity > selectedVenue.capacity) {
      this.errorMessage.set(
        `Event capacity cannot exceed venue capacity of ${selectedVenue.capacity}.`
      );
      return;
    }

    const request: CreateEventRequest = {
      name,
      description: formValue.description.trim() || null,
      venueId: formValue.venueId,
      eventCategoryId: formValue.eventCategoryId,
      startDateTimeUtc: start.toISOString(),
      endDateTimeUtc: end.toISOString(),
      ticketPrice: formValue.ticketPrice,
      parkingFee: formValue.parkingFee,
      capacity: formValue.capacity
    };

    this.saving.set(true);
    this.errorMessage.set('');

    const editingId = this.editingEventId();

    const request$ =
      editingId === null
        ? this.eventApi.create(request)
        : this.eventApi.update(editingId, request);

    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadPageData();
        },
        error: error => {
          const detail =
            error?.error?.detail ??
            'Unable to save event. Please check the event details.';

          this.errorMessage.set(detail);
        }
      });
  }

  editEvent(eventItem: EventItem): void {
    this.editingEventId.set(eventItem.id);
    this.errorMessage.set('');

    this.eventForm.setValue({
      name: eventItem.name,
      description: eventItem.description ?? '',
      venueId: eventItem.venueId,
      eventCategoryId: eventItem.eventCategoryId,
      startDateTime: this.toLocalDateTimeInput(
        eventItem.startDateTimeUtc
      ),
      endDateTime: this.toLocalDateTimeInput(
        eventItem.endDateTimeUtc
      ),
      ticketPrice: eventItem.ticketPrice,
      parkingFee: eventItem.parkingFee,
      capacity: eventItem.capacity
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  deleteEvent(eventItem: EventItem): void {
    this.pendingDeleteEvent.set(eventItem);
  }

  cancelDeleteEvent(): void {
    this.pendingDeleteEvent.set(null);
  }

  confirmDeleteEvent(): void {
    const eventItem = this.pendingDeleteEvent();

    if (eventItem === null) {
      return;
    }

    this.errorMessage.set('');

    this.eventApi.delete(eventItem.id).subscribe({
      next: () => {
        this.pendingDeleteEvent.set(null);
        this.loadPageData();
      },
      error: error => {
        this.pendingDeleteEvent.set(null);

        const detail =
          error?.error?.detail ??
          'Unable to delete event. It may be referenced by existing data.';

        this.errorMessage.set(detail);
      }
    });
  }

  private resetForm(): void {
    this.editingEventId.set(null);

    this.eventForm.reset({
      name: '',
      description: '',
      venueId: 0,
      eventCategoryId: 0,
      startDateTime: '',
      endDateTime: '',
      ticketPrice: 0,
      parkingFee: 0,
      capacity: 1
    });
  }

  private toLocalDateTimeInput(utcValue: string): string {
    const date = new Date(utcValue);
    const offsetMilliseconds =
      date.getTimezoneOffset() * 60 * 1000;

    return new Date(date.getTime() - offsetMilliseconds)
      .toISOString()
      .slice(0, 16);
  }
}
