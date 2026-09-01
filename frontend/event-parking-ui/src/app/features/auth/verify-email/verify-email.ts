import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.css'
})
export class VerifyEmail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  readonly isVerifying = signal(true);
  readonly isVerified = signal(false);
  readonly message = signal('Verifying your email address...');

  ngOnInit(): void {
    const userId =
      this.route.snapshot.queryParamMap.get('userId');

    const token =
      this.route.snapshot.queryParamMap.get('token');

    if (!userId || !token) {
      this.isVerifying.set(false);
      this.isVerified.set(false);
      this.message.set(
        'This verification link is incomplete or invalid.'
      );
      return;
    }

    this.authService
      .verifyEmail(userId, token)
      .subscribe({
        next: response => {
          this.isVerifying.set(false);
          this.isVerified.set(true);
          this.message.set(response.message);
        },
        error: (error: HttpErrorResponse) => {
          this.isVerifying.set(false);
          this.isVerified.set(false);

          if (typeof error.error?.detail === 'string') {
            this.message.set(error.error.detail);
            return;
          }

          this.message.set(
            'Unable to verify this email address.'
          );
        }
      });
  }
}
