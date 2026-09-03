import { HttpClient, HttpParams } from '@angular/common/http';
import {
  computed,
  inject,
  Injectable,
  signal
} from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  AuthSession,
  ForgotPasswordRequest,
  LoginRequest,
  MessageResponse,
  RegisterRequest,
  RegisterResponse,
  ResendVerificationRequest,
  ResetPasswordRequest
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly storageKey = 'eventParking.authSession';

  private readonly sessionState =
    signal<AuthSession | null>(this.restoreSession());

  readonly session = this.sessionState.asReadonly();

  readonly isAuthenticated = computed(() => {
    const session = this.sessionState();

    return session !== null &&
      Date.parse(session.expiresAtUtc) > Date.now();
  });

  readonly currentUser = computed(() => {
    const session = this.sessionState();

    if (!session) {
      return null;
    }

    return {
      userId: session.userId,
      fullName: session.fullName,
      email: session.email,
      role: session.role,
      accountStatus: session.accountStatus
    };
  });

  readonly role = computed(
    () => this.sessionState()?.role ?? null
  );

  register(
    request: RegisterRequest
  ): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${environment.apiBaseUrl}/auth/register`,
      request
    );
  }

  login(
    request: LoginRequest
  ): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${environment.apiBaseUrl}/auth/login`,
        request
      )
      .pipe(
        tap(response => this.storeSession(response))
      );
  }

  verifyEmail(
    userId: string,
    token: string
  ): Observable<MessageResponse> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('token', token);

    return this.http.get<MessageResponse>(
      `${environment.apiBaseUrl}/auth/verify-email`,
      { params }
    );
  }

  resendVerification(
    request: ResendVerificationRequest
  ): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${environment.apiBaseUrl}/auth/resend-verification`,
      request
    );
  }

  forgotPassword(
    request: ForgotPasswordRequest
  ): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${environment.apiBaseUrl}/auth/forgot-password`,
      request
    );
  }

  resetPassword(
    request: ResetPasswordRequest
  ): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${environment.apiBaseUrl}/auth/reset-password`,
      request
    );
  }

  getAccessToken(): string | null {
    const session = this.sessionState();

    if (!session) {
      return null;
    }

    if (Date.parse(session.expiresAtUtc) <= Date.now()) {
      this.clearSession();
      return null;
    }

    return session.accessToken;
  }

  hasRole(role: string): boolean {
    return this.isAuthenticated() &&
      this.sessionState()?.role === role;
  }

  logout(): void {
    this.clearSession();
    void this.router.navigateByUrl('/login');
  }

  private storeSession(
    response: AuthResponse
  ): void {
    const session: AuthSession = {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      userId: response.userId,
      fullName: response.fullName,
      email: response.email,
      role: response.role,
      accountStatus: response.accountStatus
    };

    this.sessionState.set(session);

    try {
      sessionStorage.setItem(
        this.storageKey,
        JSON.stringify(session)
      );
    } catch {
      // Session remains available in memory if browser storage is unavailable.
    }
  }

  private restoreSession(): AuthSession | null {
    try {
      const stored =
        sessionStorage.getItem(this.storageKey);

      if (!stored) {
        return null;
      }

      const session =
        JSON.parse(stored) as AuthSession;

      if (
        !session.accessToken ||
        !session.userId ||
        !session.role ||
        !session.expiresAtUtc ||
        Date.parse(session.expiresAtUtc) <= Date.now()
      ) {
        sessionStorage.removeItem(this.storageKey);
        return null;
      }

      return session;
    } catch {
      return null;
    }
  }

  private clearSession(): void {
    this.sessionState.set(null);

    try {
      sessionStorage.removeItem(this.storageKey);
    } catch {
      // Nothing else is required if browser storage is unavailable.
    }
  }
}
