import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-resend-verification',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './resend-verification.html',
  styleUrl: './resend-verification.css'
})
export class ResendVerification {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email,
        Validators.maxLength(256)
      ]
    ]
  });

  submit(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.authService
      .resendVerification(this.form.getRawValue())
      .subscribe({
        next: response => {
          this.isSubmitting.set(false);
          this.successMessage.set(response.message);
        },
        error: (error: HttpErrorResponse) => {
          this.isSubmitting.set(false);

          this.errorMessage.set(
            typeof error.error?.detail === 'string'
              ? error.error.detail
              : 'Unable to process the request. Please try again.'
          );
        }
      });
  }

  hasError(errorName: string): boolean {
    const control = this.form.controls.email;

    return control.touched &&
      control.hasError(errorName);
  }
}
