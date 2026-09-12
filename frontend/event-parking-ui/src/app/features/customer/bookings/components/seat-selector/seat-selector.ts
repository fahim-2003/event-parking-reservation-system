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

  @Input()
  seats: SeatResponse[] = [];

  @Input()
  selectedSeatIds: number[] = [];

  @Input()
  disabled = false;

  @Input()
  categoryName = '';

  @Output()
  seatToggled =
    new EventEmitter<SeatResponse>();


  get seatLayoutVisualClass(): string {

    switch (
      this.categoryName
        .trim()
        .toLowerCase()
    ) {

      case 'sports':
        return 'layout-sports';

      case 'concert':
        return 'layout-concert';

      case 'cinema':
        return 'layout-cinema';

      case 'conference':
        return 'layout-conference';

      case 'festival':
        return 'layout-festival';

      default:
        return 'layout-generic';
    }
  }


  get groupedSeatRows(): {
    rowLabel: string;
    seats: SeatResponse[];
  }[] {

    const rows =
      new Map<string, SeatResponse[]>();

    for (const seat of this.seats) {

      const existing =
        rows.get(seat.rowLabel) ?? [];

      existing.push(seat);

      rows.set(
        seat.rowLabel,
        existing
      );
    }

    return Array.from(
      rows.entries()
    ).map(
      ([rowLabel, seats]) => ({
        rowLabel,
        seats
      })
    );
  }


  get concertRows(): {
    rowLabel: string;
    sections: SeatResponse[][];
  }[] {

    return this.groupedSeatRows.map(row => {

      const count = row.seats.length;

      const leftEnd =
        Math.ceil(count * 0.3);

      const centreEnd =
        Math.ceil(count * 0.7);

      return {
        rowLabel: row.rowLabel,
        sections: [
          row.seats.slice(
            0,
            leftEnd
          ),
          row.seats.slice(
            leftEnd,
            centreEnd
          ),
          row.seats.slice(
            centreEnd
          )
        ].filter(
          section =>
            section.length > 0
        )
      };
    });
  }


  get cinemaRows(): {
    rowLabel: string;
    sections: SeatResponse[][];
  }[] {

    return this.groupedSeatRows.map(row => {

      const midpoint =
        Math.ceil(
          row.seats.length / 2
        );

      return {
        rowLabel: row.rowLabel,
        sections: [
          row.seats.slice(
            0,
            midpoint
          ),
          row.seats.slice(
            midpoint
          )
        ].filter(
          section =>
            section.length > 0
        )
      };
    });
  }


  get conferenceDeskRows(): {
    rowLabel: string;
    desks: SeatResponse[][];
  }[] {

    return this.groupedSeatRows.map(row => {

      const desks:
        SeatResponse[][] = [];

      for (
        let index = 0;
        index < row.seats.length;
        index += 2
      ) {
        desks.push(
          row.seats.slice(
            index,
            index + 2
          )
        );
      }

      return {
        rowLabel: row.rowLabel,
        desks
      };
    });
  }


  get festivalFanZones(): {
    name: string;
    rows: SeatResponse[][];
  }[] {

    const rows =
      this.groupedSeatRows
        .map(row => row.seats);

    if (rows.length === 0) {
      return [];
    }

    const midpoint =
      Math.ceil(
        rows.length / 2
      );

    const frontRows =
      rows.slice(
        0,
        midpoint
      );

    const rearRows =
      rows.slice(
        midpoint
      );

    const splitSide = (
      sourceRows: SeatResponse[][],
      side: 'left' | 'right'
    ): SeatResponse[][] => {

      return sourceRows.map(row => {

        const seatMidpoint =
          Math.ceil(
            row.length / 2
          );

        return side === 'left'
          ? row.slice(
              0,
              seatMidpoint
            )
          : row.slice(
              seatMidpoint
            );
      });
    };

    return [
      {
        name: 'FRONT LEFT',
        rows:
          splitSide(
            frontRows,
            'left'
          )
      },
      {
        name: 'FRONT RIGHT',
        rows:
          splitSide(
            frontRows,
            'right'
          )
      },
      {
        name: 'REAR LEFT',
        rows:
          splitSide(
            rearRows,
            'left'
          )
      },
      {
        name: 'REAR RIGHT',
        rows:
          splitSide(
            rearRows,
            'right'
          )
      }
    ];
  }


  getFestivalRowWidth(
    rowIndex: number,
    rowCount: number
  ): number {

    if (rowCount <= 1) {
      return 100;
    }

    return (
      54 +
      (
        rowIndex *
        46 /
        (rowCount - 1)
      )
    );
  }


  getSportsSeatRingClass(
    seatIndex: number
  ): string {

    return (
      `sports-ring-${(seatIndex % 3) + 1}`
    );
  }


  getSportsSeatProgress(
    seatIndex: number
  ): string {

    const ring =
      seatIndex % 3;

    const positionInRing =
      Math.floor(
        seatIndex / 3
      );

    const ringCount =
      this.seats.filter(
        (_, index) =>
          index % 3 === ring
      ).length;

    if (ringCount <= 1) {
      return '0%';
    }

    return `${
      (
        positionInRing /
        ringCount
      ) * 100
    }%`;
  }


  isSelected(
    seatId: number
  ): boolean {

    return this.selectedSeatIds
      .includes(seatId);
  }


  selectSeat(
    seat: SeatResponse
  ): void {

    if (
      this.disabled ||
      seat.status !== 'Available'
    ) {
      return;
    }

    this.seatToggled.emit(seat);
  }
}
