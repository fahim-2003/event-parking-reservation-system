import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { CustomerProfile as CustomerProfileModel } from '../../../core/models/customer.models';
import { CustomerService } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe
  ],
  templateUrl: './customer-profile.html',
  styleUrl: './customer-profile.css'
})
export class CustomerProfile implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);

  readonly profile = signal<CustomerProfileModel | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    fullName: [
      '',
      [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(150)
      ]
    ],
    phoneNumber: [
      '',
      [
        Validators.required,
        Validators.maxLength(30)
      ]
    ]
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.customerService.getOwnProfile().subscribe({
      next: profile => {
        this.profile.set(profile);

        this.form.setValue({
          fullName: profile.fullName,
          phoneNumber: profile.phoneNumber ?? ''
        });

        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading.set(false);

        this.errorMessage.set(
          this.resolveErrorMessage(
            error,
            'Unable to load your profile.'
          )
        );
      }
    });
  }

  save(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);

    this.customerService
      .updateOwnProfile(this.form.getRawValue())
      .subscribe({
        next: profile => {
          this.profile.set(profile);
          this.isSaving.set(false);
          this.successMessage.set(
            'Profile updated successfully.'
          );
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving.set(false);

          this.errorMessage.set(
            this.resolveErrorMessage(
              error,
              'Unable to update your profile.'
            )
          );
        }
      });
  }

  hasError(
    controlName: 'fullName' | 'phoneNumber',
    errorName: string
  ): boolean {
    const control = this.form.controls[controlName];

    return control.touched &&
      control.hasError(errorName);
  }

  private resolveErrorMessage(
    error: HttpErrorResponse,
    fallback: string
  ): string {
    return typeof error.error?.detail === 'string'
      ? error.error.detail
      : fallback;
  }
}
