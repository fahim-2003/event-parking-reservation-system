import { ComponentFixture, TestBed } from '@angular/core/testing';
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

  beforeEach(async () => {
    authServiceMock.forgotPassword.mockReset();

    await TestBed.configureTestingModule({
      imports: [ForgotPassword],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
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

  it('should reject an empty email', () => {
    component.submit();

    expect(component.form.invalid).toBe(true);

    expect(
      authServiceMock.forgotPassword
    ).not.toHaveBeenCalled();
  });

  it('should submit a valid email', () => {
    authServiceMock.forgotPassword.mockReturnValue(
      of({
        message:
          'If the account is eligible, password reset instructions have been sent.'
      })
    );

    component.form.setValue({
      email: 'customer1@eventparking.local'
    });

    component.submit();

    expect(
      authServiceMock.forgotPassword
    ).toHaveBeenCalledWith({
      email: 'customer1@eventparking.local'
    });

    expect(
      component.successMessage().length
    ).toBeGreaterThan(0);

    expect(component.isSubmitting()).toBe(false);
  });
});
