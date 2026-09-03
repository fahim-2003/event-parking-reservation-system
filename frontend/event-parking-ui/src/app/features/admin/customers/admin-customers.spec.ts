import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { AdminCustomers } from './admin-customers';
import { CustomerService } from '../../../core/services/customer.service';

describe('AdminCustomers', () => {
  let fixture: ComponentFixture<AdminCustomers>;
  let component: AdminCustomers;

  const customer = {
    userId: 'customer-1',
    fullName: 'Test Customer Updated',
    email: 'customer1@eventparking.local',
    phoneNumber: '0777654321',
    emailVerified: true,
    accountStatus: 'Active',
    createdAtUtc: '2026-09-01T07:46:25Z'
  };

  const customerServiceMock = {
    searchCustomers: vi.fn(),
    getCustomerForAdmin: vi.fn()
  };

  beforeEach(async () => {
    customerServiceMock.searchCustomers.mockReset();
    customerServiceMock.getCustomerForAdmin.mockReset();

    customerServiceMock.searchCustomers.mockReturnValue(
      of([customer])
    );

    await TestBed.configureTestingModule({
      imports: [AdminCustomers],
      providers: [
        {
          provide: CustomerService,
          useValue: customerServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminCustomers);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load customers', () => {
    expect(
      customerServiceMock.searchCustomers
    ).toHaveBeenCalledWith('');

    expect(component.customers().length).toBe(1);
  });

  it('should search customers', () => {
    component.searchForm.setValue({
      search: 'customer1'
    });

    component.loadCustomers();

    expect(
      customerServiceMock.searchCustomers
    ).toHaveBeenLastCalledWith('customer1');
  });

  it('should load customer details', () => {
    customerServiceMock.getCustomerForAdmin.mockReturnValue(
      of(customer)
    );

    component.viewCustomer('customer-1');

    expect(
      customerServiceMock.getCustomerForAdmin
    ).toHaveBeenCalledWith('customer-1');

    expect(component.selectedCustomer()?.email).toBe(
      'customer1@eventparking.local'
    );
  });
});
