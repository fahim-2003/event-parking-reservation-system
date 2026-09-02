export interface Venue {
  id: number;
  name: string;
  address: string;
  capacity: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateVenueRequest {
  name: string;
  address: string;
  capacity: number;
}

export interface UpdateVenueRequest {
  name: string;
  address: string;
  capacity: number;
}