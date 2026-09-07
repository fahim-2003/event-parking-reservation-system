import {
  Component,
  EventEmitter,
  Input,
  Output
} from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  SeatResponse
} from '../../services/booking.service';

import {
  SeatStatusDirective
} from '../../../../../shared/directives/seat-status.directive';

import {
  SeatLabelPipe
} from '../../../../../shared/pipes/seat-label.pipe';

@Component({
  selector: 'app-seat-selector',
  standalone: true,
  imports: [
    CommonModule,
    SeatStatusDirective,
    SeatLabelPipe
  ],
  templateUrl: './seat-selector.html',
  styleUrl: './seat-selector.css'
})
export class SeatSelector {
  @Input() seats: SeatResponse[] = [];
  @Input() selectedSeatIds: number[] = [];
  @Input() disabled = false;

  @Output()
  seatToggled =
    new EventEmitter<SeatResponse>();

  isSelected(seatId: number): boolean {
    return this.selectedSeatIds.includes(
      seatId
    );
  }

  selectSeat(seat: SeatResponse): void {
    if (
      this.disabled ||
      seat.status !== 'Available'
    ) {
      return;
    }

    this.seatToggled.emit(seat);
  }
}
