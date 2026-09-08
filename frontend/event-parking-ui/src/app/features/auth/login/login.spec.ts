import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  Router
} from '@angular/router';
import { of, throwError } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;

  const authServiceMock = {
    login: vi.fn()
  };

  const routerMock = {
    navigateByUrl: vi.fn()
  };

  const queryParameters = new Map<string, string>();

  const activatedRouteMock = {
    snapshot: {
      queryParamMap: {
        get: (key: string) =>
          queryParameters.get(key) ?? null
      }
    }
  };

  beforeEach(async () => {
    authServiceMock.login.mockReset();
    routerMock.navigateByUrl.mockReset();
    queryParameters.clear();

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
        },
        {
          provide: Router,
          useValue: routerMock
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the login component', () => {
    expect(component).toBeTruthy();
  });

  it('should reject an empty form', () => {
    component.submit();

    expect(component.loginForm.invalid).toBe(true);
    expect(authServiceMock.login).not.toHaveBeenCalled();
  });

  it('should navigate customer to customer profile', () => {
    authServiceMock.login.mockReturnValue(
      of({
        accessToken: 'test-token',
        expiresAtUtc: '2099-01-01T00:00:00Z',
        userId: 'customer-1',
        fullName: 'Test Customer',
        email: 'customer1@eventparking.local',
        role: 'Customer',
        accountStatus: 'Active'
      })
    );

    component.loginForm.setValue({
      email: 'customer1@eventparking.local',
      password: 'NewCustomer9!'
    });

    component.submit();

    expect(routerMock.navigateByUrl).toHaveBeenCalledWith(
      '/customer/dashboard'
    );
  });

  it('should navigate administrator to admin customers', () => {
    authServiceMock.login.mockReturnValue(
      of({
        accessToken: 'admin-token',
        expiresAtUtc: '2099-01-01T00:00:00Z',
        userId: 'admin-1',
        fullName: 'System Administrator',
        email: 'admin@eventparking.local',
        role: 'Administrator',
        accountStatus: 'Active'
      })
    );

    component.loginForm.setValue({
      email: 'admin@eventparking.local',
      password: 'AdminPassword9!'
    });

    component.submit();

    expect(routerMock.navigateByUrl).toHaveBeenCalledWith(
      '/admin/dashboard'
    );
  });

  it('should show invalid credential error', () => {
    authServiceMock.login.mockReturnValue(
      throwError(() => ({
        error: {
          errorCode: 'INVALID_CREDENTIALS'
        }
      }))
    );

    component.loginForm.setValue({
      email: 'customer1@eventparking.local',
      password: 'WrongPassword!'
    });

    component.submit();

    expect(component.errorMessage()).toBe(
      'Invalid email or password.'
    );

    expect(component.isSubmitting()).toBe(false);
  });
});
