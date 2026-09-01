export interface CustomerProfile {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  emailVerified: boolean;
  accountStatus: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface UpdateCustomerProfileRequest {
  fullName: string;
  phoneNumber: string;
}
