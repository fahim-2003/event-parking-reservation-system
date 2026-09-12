export type AppRole = 'Customer' | 'Administrator';

export type AccountStatus = 'Active' | 'Deactivated';

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  acceptedTerms: boolean;
}

export interface RegisterResponse {
  userId: string;
  fullName: string;
  email: string;
  role: AppRole;
  emailVerificationRequired: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  email: string;
  role: AppRole;
  accountStatus: AccountStatus;
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  email: string;
  role: AppRole;
  accountStatus: AccountStatus;
}

export interface ResendVerificationRequest {
  email: string;
}

export interface ForgotPasswordRequest {
  phoneNumber: string;
}

export interface ResetPasswordRequest {
  phoneNumber: string;
  otp: string;
  newPassword: string;
}

export interface MessageResponse {
  message: string;
}


