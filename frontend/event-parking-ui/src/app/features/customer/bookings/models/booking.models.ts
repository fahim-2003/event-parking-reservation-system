export interface CreateBookingRequest {
  eventId: number;
  seatIds: number[];
  parkingSlotId?: number | null;
}

export interface BookingResponse {
  id: number;
  bookingNumber: string;
  eventId: number;
  seatIds: number[];
  parkingSlotId?: number | null;
  status: string;
  paymentStatus: string;
  createdAt: string;
  holdExpiresAt: string;
}
