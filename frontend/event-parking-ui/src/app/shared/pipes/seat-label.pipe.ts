import {
  Pipe,
  PipeTransform
} from '@angular/core';

export interface SeatLabelValue {
  displayLabel?: string | null;
  rowLabel?: string | null;
  seatNumber?: number | null;
}

@Pipe({
  name: 'seatLabel',
  standalone: true
})
export class SeatLabelPipe implements PipeTransform {
  transform(
    seat: SeatLabelValue | null | undefined
  ): string {
    if (!seat) {
      return '';
    }

    const displayLabel =
      seat.displayLabel?.trim();

    if (displayLabel) {
      return displayLabel;
    }

    const row =
      seat.rowLabel?.trim() ?? '';

    const number =
      seat.seatNumber ?? '';

    return `${row}${number}`;
  }
}
