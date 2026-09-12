import {
  Component,
  EventEmitter,
  Input,
  Output
} from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  ParkingSlotResponse
} from '../../services/booking.service';

import {
  SlotCodePipe
} from '../../../../../shared/pipes/slot-code.pipe';

interface ParkingZoneView {
  zone: string;
  upperSlots: ParkingSlotResponse[];
  lowerSlots: ParkingSlotResponse[];
}

@Component({
  selector: 'app-parking-selector',
  standalone: true,
  imports: [
    CommonModule,
    SlotCodePipe
  ],
  templateUrl: './parking-selector.html',
  styleUrl: './parking-selector.css'
})
export class ParkingSelector {
  @Input() parkingSlots: ParkingSlotResponse[] = [];
  @Input() selectedParkingSlotId: number | null = null;
  @Input() disabled = false;

  @Output()
  parkingSelected =
    new EventEmitter<ParkingSlotResponse>();

  get parkingZones(): ParkingZoneView[] {

    const zoneMap =
      new Map<string, ParkingSlotResponse[]>();

    for (const slot of this.parkingSlots) {

      const zone =
        slot.zone?.trim().toUpperCase() || 'GENERAL';

      const slots =
        zoneMap.get(zone) ?? [];

      slots.push(slot);

      zoneMap.set(zone, slots);
    }

    return Array
      .from(zoneMap.entries())
      .sort(([left], [right]) =>
        left.localeCompare(
          right,
          undefined,
          {
            numeric: true
          }
        )
      )
      .map(([zone, slots]) => {

        const orderedSlots =
          [...slots].sort(
            (left, right) =>
              left.slotNumber - right.slotNumber
          );

        const splitAt =
          Math.ceil(
            orderedSlots.length / 2
          );

        return {
          zone,
          upperSlots:
            orderedSlots.slice(
              0,
              splitAt
            ),
          lowerSlots:
            orderedSlots.slice(
              splitAt
            )
        };

      });

  }

  get selectedParkingSlot():
    ParkingSlotResponse | null {

    if (
      this.selectedParkingSlotId === null
    ) {
      return null;
    }

    return (
      this.parkingSlots.find(
        slot =>
          slot.id ===
          this.selectedParkingSlotId
      ) ?? null
    );

  }

  isSelected(
    slotId: number
  ): boolean {

    return (
      this.selectedParkingSlotId ===
      slotId
    );

  }

  isAvailable(
    slot: ParkingSlotResponse
  ): boolean {

    return (
      slot.status === 'Available'
    );

  }

  isHeld(
    slot: ParkingSlotResponse
  ): boolean {

    return (
      slot.status === 'Held'
    );

  }

  isOccupied(
    slot: ParkingSlotResponse
  ): boolean {

    return (
      slot.status === 'Occupied'
    );

  }

  selectParking(
    slot: ParkingSlotResponse
  ): void {

    if (
      this.disabled ||
      slot.status !== 'Available'
    ) {
      return;
    }

    this.parkingSelected.emit(slot);
  }
}