import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    phoneNumber: [
      '',
      [
        Validators.required,
        Validators.pattern(/^\+?[0-9]{9,15}$/)
      ]
    ]
  });

  submit(): void {
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const phoneNumber =
      this.form.controls.phoneNumber.value.trim();

    this.authService
      .forgotPassword({ phoneNumber })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);

          void this.router.navigate(
            ['/reset-password'],
            {
              queryParams: {
                phoneNumber
              }
            }
          );
        },
        error: (error: HttpErrorResponse) => {
          this.isSubmitting.set(false);

          this.errorMessage.set(
            typeof error.error?.detail === 'string'
              ? error.error.detail
              : 'Unable to send the password reset OTP.'
          );
        }
      });
  }

  hasError(errorName: string): boolean {
    const control = this.form.controls.phoneNumber;

    return control.touched &&
      control.hasError(errorName);
  }
}
