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
      slot.zone?.trim() ?? '';

    const number =
      slot.slotNumber ?? '';

    return `${zone}${number}`;
  }
}
