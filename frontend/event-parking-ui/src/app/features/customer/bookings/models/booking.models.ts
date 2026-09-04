export interface CreateBookingRequest {
  eventId: number;
  seatId?: number | null;
  parkingSlotId?: number | null;
}

export interface BookingResponse {
  id: number;
  eventId: number;
  seatId?: number | null;
  parkingSlotId?: number | null;
  status: string;
}