import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EventApiService } from '../../../events/services/event-api.service';
import { Seat } from '../../models/seat.model';
import { SeatApiService } from '../../services/seat-api.service';
import { SeatLayout } from './seat-layout';

describe('SeatLayout', () => {
  let fixture: ComponentFixture<SeatLayout>;
  let component: SeatLayout;

  const eventApi = {
    getAll: vi.fn()
  };

  const seatApi = {
    getByEvent: vi.fn(),
    generate: vi.fn(),
    update: vi.fn()
  };

  const generatedSeats: Seat[] = [
    {
      id: 1,
      eventId: 10,
      rowLabel: 'A',
      seatNumber: 1,
      displayLabel: 'A1',
      status: 'Available',
      rowVersion: 'AQ=='
    },
    {
      id: 2,
      eventId: 10,
      rowLabel: 'A',
      seatNumber: 2,
      displayLabel: 'A2',
      status: 'Available',
      rowVersion: 'Ag=='
    },
    {
      id: 3,
      eventId: 10,
      rowLabel: 'B',
      seatNumber: 1,
      displayLabel: 'B1',
      status: 'Available',
      rowVersion: 'Aw=='
    },
    {
      id: 4,
      eventId: 10,
      rowLabel: 'B',
      seatNumber: 2,
      displayLabel: 'B2',
      status: 'Available',
      rowVersion: 'BA=='
    }
  ];

  beforeEach(async () => {
    eventApi.getAll.mockReturnValue(
      of([
        {
          id: 10,
          name: 'Test Event',
          description: null,
          venueId: 1,
          venueName: 'Main Hall',
          eventCategoryId: 1,
          categoryName: 'Conference',
          startDateTimeUtc: '2027-01-01T09:00:00Z',
          endDateTimeUtc: '2027-01-01T12:00:00Z',
          ticketPrice: 500,
          parkingFee: 100,
          capacity: 4,
          createdAtUtc: '2026-09-02T00:00:00Z',
          updatedAtUtc: '2026-09-02T00:00:00Z'
        }
      ])
    );

    seatApi.getByEvent.mockReturnValue(of([]));
    seatApi.generate.mockReturnValue(of(generatedSeats));
    seatApi.update.mockReturnValue(of(generatedSeats[0]));

    await TestBed.configureTestingModule({
      imports: [SeatLayout],
      providers: [
        {
          provide: EventApiService,
          useValue: eventApi
        },
        {
          provide: SeatApiService,
          useValue: seatApi
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SeatLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('creates the seat layout component', () => {
    expect(component).toBeTruthy();
    expect(component.events().length).toBe(1);
  });

  it('loads seats when an event is selected', () => {
    component.layoutForm.controls.eventId.setValue(10);

    component.eventChanged();

    expect(seatApi.getByEvent).toHaveBeenCalledWith(10);
  });

  it('generates a seat map when seat count matches event capacity', () => {
    component.layoutForm.setValue({
      eventId: 10,
      rows: 2,
      seatsPerRow: 2
    });

    component.generateLayout();

    expect(seatApi.generate).toHaveBeenCalledWith(
      10,
      {
        rows: 2,
        seatsPerRow: 2
      }
    );

    expect(component.seats().length).toBe(4);
  });

  it('rejects a seat map that does not match event capacity', () => {
    component.layoutForm.setValue({
      eventId: 10,
      rows: 1,
      seatsPerRow: 2
    });

    component.generateLayout();

    expect(seatApi.generate).not.toHaveBeenCalled();
    expect(component.errorMessage()).toContain(
      'exactly 4 seats'
    );
  });

  it('does not allow a held seat to enter edit mode', () => {
    component.editSeat({
      ...generatedSeats[0],
      status: 'Held'
    });

    expect(component.editingSeatId()).toBeNull();
  });
});
