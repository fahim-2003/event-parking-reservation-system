import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { CustomerProfile } from './customer-profile';
import { CustomerService } from '../../../core/services/customer.service';

describe('CustomerProfile', () => {
  let fixture: ComponentFixture<CustomerProfile>;
  let component: CustomerProfile;

  const profile = {
    userId: 'customer-1',
    fullName: 'Test Customer Updated',
    email: 'customer1@eventparking.local',
    phoneNumber: '0777654321',
    emailVerified: true,
    accountStatus: 'Active',
    createdAtUtc: '2026-09-01T07:46:25Z',
    updatedAtUtc: '2026-09-01T11:00:00Z'
  };

  const customerServiceMock = {
    getOwnProfile: vi.fn(),
    updateOwnProfile: vi.fn()
  };

  beforeEach(async () => {
    customerServiceMock.getOwnProfile.mockReset();
    customerServiceMock.updateOwnProfile.mockReset();

    customerServiceMock.getOwnProfile.mockReturnValue(
      of(profile)
    );

    await TestBed.configureTestingModule({
      imports: [CustomerProfile],
      providers: [
        {
          provide: CustomerService,
          useValue: customerServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerProfile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load the current customer profile', () => {
    expect(
      customerServiceMock.getOwnProfile
    ).toHaveBeenCalled();

    expect(component.profile()?.email).toBe(
      'customer1@eventparking.local'
    );

    expect(component.form.controls.fullName.value).toBe(
      'Test Customer Updated'
    );
  });

  it('should reject an invalid profile form', () => {
    component.form.setValue({
      fullName: '',
      phoneNumber: ''
    });

    component.save();

    expect(
      customerServiceMock.updateOwnProfile
    ).not.toHaveBeenCalled();
  });

  it('should update the customer profile', () => {
    const updatedProfile = {
      ...profile,
      fullName: 'Updated Customer',
      phoneNumber: '0771112233'
    };

    customerServiceMock.updateOwnProfile.mockReturnValue(
      of(updatedProfile)
    );

    component.form.setValue({
      fullName: 'Updated Customer',
      phoneNumber: '0771112233'
    });

    component.save();

    expect(
      customerServiceMock.updateOwnProfile
    ).toHaveBeenCalledWith({
      fullName: 'Updated Customer',
      phoneNumber: '0771112233'
    });

    expect(component.profile()?.fullName).toBe(
      'Updated Customer'
    );

    expect(component.successMessage()).toContain(
      'successfully'
    );
  });
});
