import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EventApiService } from '../../../events/services/event-api.service';
import { ParkingSlot } from '../../models/parking-slot.model';
import { ParkingApiService } from '../../services/parking-api.service';
import { ParkingLayout } from './parking-layout';

describe('ParkingLayout', () => {
  let fixture: ComponentFixture<ParkingLayout>;
  let component: ParkingLayout;

  const eventApi = {
    getAll: vi.fn()
  };

  const parkingApi = {
    getByEvent: vi.fn(),
    generate: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  };

  const slots: ParkingSlot[] = [
    {
      id: 1,
      eventId: 10,
      zone: 'A',
      slotNumber: 1,
      status: 'Available',
      rowVersion: 'AQ=='
    },
    {
      id: 2,
      eventId: 10,
      zone: 'A',
      slotNumber: 2,
      status: 'Available',
      rowVersion: 'Ag=='
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
          capacity: 100,
          createdAtUtc: '2026-09-02T00:00:00Z',
          updatedAtUtc: '2026-09-02T00:00:00Z'
        }
      ])
    );

    parkingApi.getByEvent.mockReturnValue(of(slots));
    parkingApi.generate.mockReturnValue(of(slots));
    parkingApi.update.mockReturnValue(of(slots[0]));
    parkingApi.delete.mockReturnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [ParkingLayout],
      providers: [
        {
          provide: EventApiService,
          useValue: eventApi
        },
        {
          provide: ParkingApiService,
          useValue: parkingApi
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ParkingLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('creates the parking layout component', () => {
    expect(component).toBeTruthy();
    expect(component.events().length).toBe(1);
  });

  it('loads parking slots when an event is selected', () => {
    component.layoutForm.controls.eventId.setValue(10);

    component.eventChanged();

    expect(parkingApi.getByEvent).toHaveBeenCalledWith(10);
  });

  it('generates a parking zone', () => {
    component.layoutForm.setValue({
      eventId: 10,
      zone: 'a',
      numberOfSlots: 2
    });

    component.generateLayout();

    expect(parkingApi.generate).toHaveBeenCalledWith(
      10,
      {
        zone: 'A',
        numberOfSlots: 2
      }
    );
  });

  it('does not edit an occupied parking slot', () => {
    component.editSlot({
      ...slots[0],
      status: 'Occupied'
    });

    expect(component.editingSlotId()).toBeNull();
  });

  it('marks an available parking slot for deletion', () => {
    component.deleteSlot(slots[0]);

    expect(component.pendingDeleteSlot()?.id).toBe(1);
  });
});
