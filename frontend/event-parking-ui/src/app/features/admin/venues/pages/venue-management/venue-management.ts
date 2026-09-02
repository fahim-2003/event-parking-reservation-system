import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';
import { ConfirmationDialog } from '../../../../../shared/components/confirmation-dialog/confirmation-dialog';
import {
  CreateVenueRequest,
  Venue
} from '../../models/venue.model';
import { VenueApiService } from '../../services/venue-api.service';

@Component({
  selector: 'app-venue-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ConfirmationDialog
  ],
  templateUrl: './venue-management.html',
  styleUrl: './venue-management.css'
})
export class VenueManagement {
  private readonly venueApi = inject(VenueApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly venues = signal<Venue[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingVenueId = signal<number | null>(null);
  readonly pendingDeleteVenue = signal<Venue | null>(null);

  readonly venueForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    address: ['', [Validators.required]],
    capacity: [1, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    this.loadVenues();
  }

  loadVenues(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.venueApi
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: venues => this.venues.set(venues),
        error: () =>
          this.errorMessage.set('Unable to load venues. Please try again.')
      });
  }

  submit(): void {
    if (this.venueForm.invalid) {
      this.venueForm.markAllAsTouched();
      return;
    }

    const formValue = this.venueForm.getRawValue();

    const request: CreateVenueRequest = {
      name: formValue.name.trim(),
      address: formValue.address.trim(),
      capacity: formValue.capacity
    };

    if (!request.name || !request.address) {
      this.venueForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    const editingId = this.editingVenueId();

    const request$ =
      editingId === null
        ? this.venueApi.create(request)
        : this.venueApi.update(editingId, request);

    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadVenues();
        },
        error: () =>
          this.errorMessage.set('Unable to save venue. Please try again.')
      });
  }

  editVenue(venue: Venue): void {
    this.editingVenueId.set(venue.id);

    this.venueForm.setValue({
      name: venue.name,
      address: venue.address,
      capacity: venue.capacity
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  deleteVenue(venue: Venue): void {
    this.pendingDeleteVenue.set(venue);
  }

  cancelDeleteVenue(): void {
    this.pendingDeleteVenue.set(null);
  }

  confirmDeleteVenue(): void {
    const venue = this.pendingDeleteVenue();

    if (venue === null) {
      return;
    }

    this.errorMessage.set('');

    this.venueApi.delete(venue.id).subscribe({
      next: () => {
        this.pendingDeleteVenue.set(null);
        this.loadVenues();
      },
      error: () => {
        this.pendingDeleteVenue.set(null);
        this.errorMessage.set(
          'Unable to delete venue. It may be referenced by existing data.'
        );
      }
    });
  }

  private resetForm(): void {
    this.editingVenueId.set(null);

    this.venueForm.reset({
      name: '',
      address: '',
      capacity: 1
    });
  }
}
