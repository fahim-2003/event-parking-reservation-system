import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { Register } from './register';
import { AuthService } from '../../../core/services/auth.service';

describe('Register', () => {
  let fixture: ComponentFixture<Register>;
  let component: Register;

  const authServiceMock = {
    register: vi.fn()
  };

  beforeEach(async () => {
    authServiceMock.register.mockReset();

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the register component', () => {
    expect(component).toBeTruthy();
  });

  it('should reject an empty form', () => {
    component.submit();

    expect(component.registerForm.invalid).toBe(true);
    expect(authServiceMock.register).not.toHaveBeenCalled();
  });

  it('should reject mismatched passwords', () => {
    component.registerForm.patchValue({
      fullName: 'New Customer',
      email: 'newcustomer@eventparking.local',
      phoneNumber: '0771112233',
      password: 'Customer9!',
      confirmPassword: 'Different9!',
      acceptedTerms: true
    });

    component.submit();

    expect(
      component.registerForm.hasError('passwordsMismatch')
    ).toBe(true);

    expect(authServiceMock.register).not.toHaveBeenCalled();
  });

  it('should submit valid registration data', () => {
    authServiceMock.register.mockReturnValue(
      of({
        userId: 'new-customer-id',
        fullName: 'New Customer',
        email: 'newcustomer@eventparking.local',
        role: 'Customer',
        emailVerificationRequired: true
      })
    );

    component.registerForm.setValue({
      fullName: 'New Customer',
      email: 'newcustomer@eventparking.local',
      phoneNumber: '0771112233',
      password: 'Customer9!',
      confirmPassword: 'Customer9!',
      acceptedTerms: true
    });

    component.submit();

    expect(authServiceMock.register).toHaveBeenCalled();

    expect(component.successMessage()).toContain(
      'Verify your email'
    );

    expect(component.isSubmitting()).toBe(false);
  });

  it('should show duplicate email error', () => {
    authServiceMock.register.mockReturnValue(
      throwError(() => ({
        error: {
          errorCode: 'EMAIL_ALREADY_REGISTERED'
        }
      }))
    );

    component.registerForm.setValue({
      fullName: 'New Customer',
      email: 'customer1@eventparking.local',
      phoneNumber: '0771112233',
      password: 'Customer9!',
      confirmPassword: 'Customer9!',
      acceptedTerms: true
    });

    component.submit();

    expect(component.errorMessage()).toContain(
      'already exists'
    );

    expect(component.isSubmitting()).toBe(false);
  });
});
