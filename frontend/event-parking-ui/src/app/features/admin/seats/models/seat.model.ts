export interface Seat {
  id: number;
  eventId: number;
  rowLabel: string;
  seatNumber: number;
  displayLabel: string;
  status: 'Available' | 'Held' | 'Booked';
  rowVersion: string;
}

export interface GenerateSeatMapRequest {
  rows: number;
  seatsPerRow: number;
}

export interface UpdateSeatRequest {
  rowLabel: string;
  seatNumber: number;
  rowVersion: string;
}
