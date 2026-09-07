import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
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
      .forgotPassword(this.form.getRawValue())
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
              : 'Unable to process the password reset request.'
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
