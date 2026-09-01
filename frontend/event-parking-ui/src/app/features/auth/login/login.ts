import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  ActivatedRoute,
  Router
} from '@angular/router';
import { AppRole } from '../../../core/models/auth.models';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly loginForm = this.formBuilder.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email,
        Validators.maxLength(256)
      ]
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.maxLength(128)
      ]
    ]
  });

  submit(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.authService.login(
      this.loginForm.getRawValue()
    ).subscribe({
      next: response => {
        this.isSubmitting.set(false);

        const destination =
          this.resolveDestination(response.role);

        void this.router.navigateByUrl(destination);
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

  hasError(
    controlName: 'email' | 'password',
    errorName: string
  ): boolean {
    const control = this.loginForm.controls[controlName];

    return control.touched &&
      control.hasError(errorName);
  }

  private resolveDestination(role: AppRole): string {
    const returnUrl =
      this.route.snapshot.queryParamMap.get('returnUrl');

    if (
      role === 'Customer' &&
      returnUrl?.startsWith('/customer/')
    ) {
      return returnUrl;
    }

    if (
      role === 'Administrator' &&
      returnUrl?.startsWith('/admin/')
    ) {
      return returnUrl;
    }

    return role === 'Administrator'
      ? '/admin/customers'
      : '/customer/profile';
  }

  private resolveErrorMessage(
    error: HttpErrorResponse
  ): string {
    const errorCode =
      error.error?.errorCode as string | undefined;

    if (errorCode === 'EMAIL_NOT_VERIFIED') {
      return 'Verify your email address before signing in.';
    }

    if (errorCode === 'INVALID_CREDENTIALS') {
      return 'Invalid email or password.';
    }

    if (typeof error.error?.detail === 'string') {
      return error.error.detail;
    }

    return 'Unable to sign in. Please try again.';
  }
}
