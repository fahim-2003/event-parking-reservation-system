import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { ForgotPassword } from './forgot-password';
import { AuthService } from '../../../core/services/auth.service';

describe('ForgotPassword', () => {
  let fixture: ComponentFixture<ForgotPassword>;
  let component: ForgotPassword;

  const authServiceMock = {
    forgotPassword: vi.fn()
  };

  const routerMock = {
    navigate: vi.fn()
  };

  beforeEach(async () => {
    authServiceMock.forgotPassword.mockReset();
    routerMock.navigate.mockReset();

    await TestBed.configureTestingModule({
      imports: [ForgotPassword],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
        },
        {
          provide: Router,
          useValue: routerMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ForgotPassword);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should reject an empty phone number', () => {
    component.submit();

    expect(component.form.invalid).toBe(true);

    expect(
      authServiceMock.forgotPassword
    ).not.toHaveBeenCalled();
  });

  it('should submit a valid phone number and navigate to reset', () => {
    authServiceMock.forgotPassword.mockReturnValue(
      of({
        message:
          'If an eligible account exists, a password reset OTP has been generated.'
      })
    );

    component.form.setValue({
      phoneNumber: '0773964123'
    });

    component.submit();

    expect(
      authServiceMock.forgotPassword
    ).toHaveBeenCalledWith({
      phoneNumber: '0773964123'
    });

    expect(routerMock.navigate).toHaveBeenCalledWith(
      ['/reset-password'],
      {
        queryParams: {
          phoneNumber: '0773964123'
        }
      }
    );

    expect(component.isSubmitting()).toBe(false);
  });
});
