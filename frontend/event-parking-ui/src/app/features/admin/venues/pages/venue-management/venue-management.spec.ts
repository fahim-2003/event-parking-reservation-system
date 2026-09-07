import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Venue } from '../../models/venue.model';
import { VenueApiService } from '../../services/venue-api.service';
import { VenueManagement } from './venue-management';

describe('VenueManagement', () => {
  const venues: Venue[] = [
    {
      id: 1,
      name: 'Main Hall',
      address: '10 Central Road',
      capacity: 500,
      createdAtUtc: '2026-09-01T00:00:00Z',
      updatedAtUtc: '2026-09-01T00:00:00Z'
    }
  ];

  let createdRequest: unknown;
  let deletedVenueId: number | null;

  const venueApiMock = {
    getAll: () => of(venues),

    getById: (venueId: number) =>
      of(venues.find(venue => venue.id === venueId)!),

    create: (request: unknown) => {
      createdRequest = request;

      return of({
        ...venues[0],
        ...(request as object)
      });
    },

    update: (_venueId: number, request: unknown) =>
      of({
        ...venues[0],
        ...(request as object)
      }),

    delete: (venueId: number) => {
      deletedVenueId = venueId;
      return of(void 0);
    }
  };

  beforeEach(async () => {
    createdRequest = null;
    deletedVenueId = null;

    await TestBed.configureTestingModule({
      imports: [VenueManagement],
      providers: [
        {
          provide: VenueApiService,
          useValue: venueApiMock
        }
      ]
    }).compileComponents();
  });

  it('should create the component and load venues', () => {
    const fixture = TestBed.createComponent(VenueManagement);
    const component = fixture.componentInstance;

    expect(component).toBeTruthy();
    expect(component.venues()).toEqual(venues);
    expect(component.loading()).toBe(false);
  });

  it('should trim venue values before create', () => {
    const fixture = TestBed.createComponent(VenueManagement);
    const component = fixture.componentInstance;

    component.venueForm.setValue({
      name: '  Main Hall  ',
      address: '  10 Central Road  ',
      capacity: 500
    });

    component.submit();

    expect(createdRequest).toEqual({
      name: 'Main Hall',
      address: '10 Central Road',
      capacity: 500
    });
  });

  it('should require confirmation before deleting a venue', () => {
    const fixture = TestBed.createComponent(VenueManagement);
    const component = fixture.componentInstance;

    component.deleteVenue(venues[0]);

    expect(component.pendingDeleteVenue()).toEqual(venues[0]);
    expect(deletedVenueId).toBeNull();

    component.confirmDeleteVenue();

    expect(deletedVenueId).toBe(1);
    expect(component.pendingDeleteVenue()).toBeNull();
  });
});
