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

  isSelected(slotId: number): boolean {
    return this.selectedParkingSlotId === slotId;
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
