export interface ParkingSlot {
  id: number;
  eventId: number;
  zone: string;
  slotNumber: number;
  status: 'Available' | 'Held' | 'Occupied';
  rowVersion: string;
}

export interface GenerateParkingLayoutRequest {
  zone: string;
  numberOfSlots: number;
}

export interface UpdateParkingSlotRequest {
  zone: string;
  slotNumber: number;
  rowVersion: string;
}
