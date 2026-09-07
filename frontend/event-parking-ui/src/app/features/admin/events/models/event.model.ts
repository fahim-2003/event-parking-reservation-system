export interface EventItem {
  id: number;
  name: string;
  description: string | null;
  venueId: number;
  venueName: string;
  eventCategoryId: number;
  categoryName: string;
  startDateTimeUtc: string;
  endDateTimeUtc: string;
  ticketPrice: number;
  parkingFee: number;
  capacity: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateEventRequest {
  name: string;
  description: string | null;
  venueId: number;
  eventCategoryId: number;
  startDateTimeUtc: string;
  endDateTimeUtc: string;
  ticketPrice: number;
  parkingFee: number;
  capacity: number;
}

export interface UpdateEventRequest {
  name: string;
  description: string | null;
  venueId: number;
  eventCategoryId: number;
  startDateTimeUtc: string;
  endDateTimeUtc: string;
  ticketPrice: number;
  parkingFee: number;
  capacity: number;
}

export interface EventFilters {
  search?: string;
  dateFromUtc?: string;
  dateToUtc?: string;
  venueId?: number;
  categoryId?: number;
}
