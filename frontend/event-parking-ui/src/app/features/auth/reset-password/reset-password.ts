import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  signal
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const newPassword =
    control.get('newPassword')?.value;

  const confirmPassword =
    control.get('confirmPassword')?.value;

  return newPassword === confirmPassword
    ? null
    : { passwordsMismatch: true };
};

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly userId =
    this.route.snapshot.queryParamMap.get('userId') ?? '';

  readonly token =
    this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly hasValidLink =
    this.userId.length > 0 &&
    this.token.length > 0;

  readonly form = this.formBuilder.nonNullable.group(
    {
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(128)
        ]
      ],
      confirmPassword: [
        '',
        [
          Validators.required,
          Validators.maxLength(128)
        ]
      ]
    },
    {
      validators: passwordsMatchValidator
    }
  );

  submit(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    if (!this.hasValidLink) {
      this.errorMessage.set(
        'This password reset link is incomplete or invalid.'
      );
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const values = this.form.getRawValue();

    this.authService.resetPassword({
      userId: this.userId,
      token: this.token,
      newPassword: values.newPassword
    }).subscribe({
      next: response => {
        this.isSubmitting.set(false);
        this.successMessage.set(response.message);

        this.form.reset({
          newPassword: '',
          confirmPassword: ''
        });
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);

        this.errorMessage.set(
          typeof error.error?.detail === 'string'
            ? error.error.detail
            : 'Unable to reset the password. The link may be invalid or expired.'
        );
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(value => !value);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(value => !value);
  }

  hasError(
    controlName: 'newPassword' | 'confirmPassword',
    errorName: string
  ): boolean {
    const control = this.form.controls[controlName];

    return control.touched &&
      control.hasError(errorName);
  }

  passwordsMismatch(): boolean {
    return this.form.hasError('passwordsMismatch') &&
      this.form.controls.confirmPassword.touched;
  }
}
