import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { describe, expect, it, beforeEach, vi } from 'vitest';

import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;

  const authServiceMock = {
    login: vi.fn()
  };

  beforeEach(async () => {
    authServiceMock.login.mockReset();

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
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

  it('should submit valid credentials', () => {
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

    expect(authServiceMock.login).toHaveBeenCalledWith({
      email: 'customer1@eventparking.local',
      password: 'NewCustomer9!'
    });

    expect(component.successMessage()).toContain(
      'Test Customer'
    );

    expect(component.isSubmitting()).toBe(false);
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
