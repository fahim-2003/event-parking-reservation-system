import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EventCategory } from '../../../categories/models/category.model';
import { CategoryApiService } from '../../../categories/services/category-api.service';
import { Venue } from '../../../venues/models/venue.model';
import { VenueApiService } from '../../../venues/services/venue-api.service';
import {
  CreateEventRequest,
  EventItem
} from '../../models/event.model';
import { EventApiService } from '../../services/event-api.service';
import { EventManagement } from './event-management';

describe('EventManagement', () => {
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

  const categories: EventCategory[] = [
    {
      id: 1,
      name: 'Conference',
      createdAtUtc: '2026-09-01T00:00:00Z',
      updatedAtUtc: '2026-09-01T00:00:00Z'
    }
  ];

  const events: EventItem[] = [
    {
      id: 1,
      name: 'Tech Conference',
      description: 'Annual technology event',
      venueId: 1,
      venueName: 'Main Hall',
      eventCategoryId: 1,
      categoryName: 'Conference',
      startDateTimeUtc: '2026-10-10T09:00:00Z',
      endDateTimeUtc: '2026-10-10T17:00:00Z',
      ticketPrice: 1500,
      parkingFee: 200,
      capacity: 400,
      createdAtUtc: '2026-09-01T00:00:00Z',
      updatedAtUtc: '2026-09-01T00:00:00Z'
    }
  ];

  let createdRequest: CreateEventRequest | null;
  let deletedEventId: number | null;

  const eventApiMock = {
    getAll: () => of(events),

    getById: (eventId: number) =>
      of(events.find(eventItem => eventItem.id === eventId)!),

    create: (request: CreateEventRequest) => {
      createdRequest = request;

      return of({
        ...events[0],
        ...request
      });
    },

    update: (_eventId: number, request: CreateEventRequest) =>
      of({
        ...events[0],
        ...request
      }),

    delete: (eventId: number) => {
      deletedEventId = eventId;
      return of(void 0);
    }
  };

  const venueApiMock = {
    getAll: () => of(venues)
  };

  const categoryApiMock = {
    getAll: () => of(categories)
  };

  beforeEach(async () => {
    createdRequest = null;
    deletedEventId = null;

    await TestBed.configureTestingModule({
      imports: [EventManagement],
      providers: [
        {
          provide: EventApiService,
          useValue: eventApiMock
        },
        {
          provide: VenueApiService,
          useValue: venueApiMock
        },
        {
          provide: CategoryApiService,
          useValue: categoryApiMock
        }
      ]
    }).compileComponents();
  });

  it('should create component and load management data', () => {
    const fixture = TestBed.createComponent(EventManagement);
    const component = fixture.componentInstance;

    expect(component).toBeTruthy();
    expect(component.events()).toEqual(events);
    expect(component.venues()).toEqual(venues);
    expect(component.categories()).toEqual(categories);
    expect(component.loading()).toBe(false);
  });

  it('should trim values before creating an event', () => {
    const fixture = TestBed.createComponent(EventManagement);
    const component = fixture.componentInstance;

    component.eventForm.setValue({
      name: '  Tech Conference  ',
      description: '  Annual technology event  ',
      venueId: 1,
      eventCategoryId: 1,
      startDateTime: '2026-10-10T09:00',
      endDateTime: '2026-10-10T17:00',
      ticketPrice: 1500,
      parkingFee: 200,
      capacity: 400
    });

    component.submit();

    expect(createdRequest).not.toBeNull();
    expect(createdRequest?.name).toBe('Tech Conference');
    expect(createdRequest?.description).toBe(
      'Annual technology event'
    );
    expect(createdRequest?.venueId).toBe(1);
    expect(createdRequest?.eventCategoryId).toBe(1);
    expect(createdRequest?.capacity).toBe(400);
  });

  it('should reject capacity greater than selected venue capacity', () => {
    const fixture = TestBed.createComponent(EventManagement);
    const component = fixture.componentInstance;

    component.eventForm.setValue({
      name: 'Large Event',
      description: '',
      venueId: 1,
      eventCategoryId: 1,
      startDateTime: '2026-10-10T09:00',
      endDateTime: '2026-10-10T17:00',
      ticketPrice: 100,
      parkingFee: 50,
      capacity: 501
    });

    component.submit();

    expect(createdRequest).toBeNull();
    expect(component.errorMessage()).toContain(
      'cannot exceed venue capacity'
    );
  });

  it('should require confirmation before deleting an event', () => {
    const fixture = TestBed.createComponent(EventManagement);
    const component = fixture.componentInstance;

    component.deleteEvent(events[0]);

    expect(component.pendingDeleteEvent()).toEqual(events[0]);
    expect(deletedEventId).toBeNull();

    component.confirmDeleteEvent();

    expect(deletedEventId).toBe(1);
    expect(component.pendingDeleteEvent()).toBeNull();
  });
});
