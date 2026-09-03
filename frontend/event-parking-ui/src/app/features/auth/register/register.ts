import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword
    ? null
    : { passwordsMismatch: true };
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly isSubmitting = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly registerForm = this.formBuilder.nonNullable.group(
    {
      fullName: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(150)
        ]
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.maxLength(256)
        ]
      ],
      phoneNumber: [
        '',
        [
          Validators.required,
          Validators.maxLength(30)
        ]
      ],
      password: [
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
      ],
      acceptedTerms: [
        false,
        [
          Validators.requiredTrue
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

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.authService.register(
      this.registerForm.getRawValue()
    ).subscribe({
      next: response => {
        this.isSubmitting.set(false);

        this.successMessage.set(
          response.emailVerificationRequired
            ? 'Account created. Verify your email before signing in.'
            : 'Account created successfully.'
        );

        this.registerForm.controls.password.reset('');
        this.registerForm.controls.confirmPassword.reset('');
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(
          this.resolveErrorMessage(error)
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
    controlName:
      | 'fullName'
      | 'email'
      | 'phoneNumber'
      | 'password'
      | 'confirmPassword'
      | 'acceptedTerms',
    errorName: string
  ): boolean {
    const control =
      this.registerForm.controls[controlName];

    return control.touched &&
      control.hasError(errorName);
  }

  passwordsMismatch(): boolean {
    return this.registerForm.hasError(
      'passwordsMismatch'
    ) &&
      this.registerForm.controls.confirmPassword.touched;
  }

  private resolveErrorMessage(
    error: HttpErrorResponse
  ): string {
    const errorCode =
      error.error?.errorCode as string | undefined;

    if (errorCode === 'EMAIL_ALREADY_REGISTERED') {
      return 'An account already exists for this email address.';
    }

    if (typeof error.error?.detail === 'string') {
      return error.error.detail;
    }

    return 'Unable to create the account. Please try again.';
  }
}
