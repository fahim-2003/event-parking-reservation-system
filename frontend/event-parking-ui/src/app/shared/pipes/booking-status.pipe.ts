import {
  Pipe,
  PipeTransform
} from '@angular/core';

@Pipe({
  name: 'bookingStatus',
  standalone: true
})
export class BookingStatusPipe implements PipeTransform {
  transform(status: string | null | undefined): string {
    if (!status) {
      return 'Unknown';
    }

    switch (status.trim().toLowerCase()) {
      case 'held':
        return 'Held';
      case 'confirmed':
        return 'Confirmed';
      case 'cancelled':
        return 'Cancelled';
      case 'expired':
        return 'Expired';
      default:
        return status;
    }
  }
}
