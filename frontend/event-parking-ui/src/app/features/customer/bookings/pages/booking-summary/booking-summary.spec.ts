import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { BookingService } from '../../services/booking.service';
import { BookingSummary } from './booking-summary';

describe('BookingSummary', () => {
  let component: BookingSummary;
  let fixture: ComponentFixture<BookingSummary>;

  const bookingServiceMock = {
    getMine: () => of([]),
    cancel: () => of({
      id: 1,
      bookingNumber: 'BK-TEST-001',
      eventId: 1,
      seatIds: [1],
      parkingSlotId: null,
      status: 'Cancelled',
      paymentStatus: 'Pending',
      createdAt: new Date().toISOString(),
      holdExpiresAt: new Date().toISOString()
    })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookingSummary],
      providers: [
        {
          provide: BookingService,
          useValue: bookingServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingSummary);
    component = fixture.componentInstance;

    fixture.detectChanges();

    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load bookings without making a real HTTP request', () => {
    expect(component.bookings).toEqual([]);
    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('');
  });
});
