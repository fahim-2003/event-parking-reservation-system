import {
  Pipe,
  PipeTransform
} from '@angular/core';

export interface ParkingSlotCodeValue {
  zone?: string | null;
  slotNumber?: number | null;
}

@Pipe({
  name: 'slotCode',
  standalone: true
})
export class SlotCodePipe implements PipeTransform {

  transform(
    slot: ParkingSlotCodeValue | null | undefined
  ): string {

    if (!slot) {
      return '';
    }

    const zone =
      slot.zone?.trim().toUpperCase() ?? '';

    const number =
      slot.slotNumber;

    if (
      !zone &&
      (number === null || number === undefined)
    ) {
      return '';
    }

    if (
      number === null ||
      number === undefined
    ) {
      return zone;
    }

    const formattedNumber =
      String(number).padStart(2, '0');

    return zone
      ? `${zone}-${formattedNumber}`
      : formattedNumber;
  }
}