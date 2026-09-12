import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { ResetPassword } from './reset-password';
import { AuthService } from '../../../core/services/auth.service';

describe('ResetPassword', () => {
  let fixture: ComponentFixture<ResetPassword>;
  let component: ResetPassword;

  const authServiceMock = {
    resetPassword: vi.fn()
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

  beforeEach(() => {
    authServiceMock.resetPassword.mockReset();
    queryParameters.clear();
  });

  async function createComponent(): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [ResetPassword],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ResetPassword);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should reject an incomplete reset request', async () => {
    await createComponent();

    expect(component.hasValidRequest).toBe(false);

    component.submit();

    expect(
      authServiceMock.resetPassword
    ).not.toHaveBeenCalled();

    expect(component.errorMessage()).toContain(
      'incomplete'
    );
  });

  it('should reject mismatched passwords', async () => {
    queryParameters.set(
      'phoneNumber',
      '0773964123'
    );

    await createComponent();

    component.form.setValue({
      otp: '123456',
      newPassword: 'NewPassword9!',
      confirmPassword: 'DifferentPassword9!'
    });

    component.submit();

    expect(
      component.form.hasError('passwordsMismatch')
    ).toBe(true);

    expect(
      authServiceMock.resetPassword
    ).not.toHaveBeenCalled();
  });

  it('should submit a valid password reset', async () => {
    queryParameters.set(
      'phoneNumber',
      '0773964123'
    );

    authServiceMock.resetPassword.mockReturnValue(
      of({
        message: 'Password reset successfully.'
      })
    );

    await createComponent();

    component.form.setValue({
      otp: '123456',
      newPassword: 'NewPassword9!',
      confirmPassword: 'NewPassword9!'
    });

    component.submit();

    expect(
      authServiceMock.resetPassword
    ).toHaveBeenCalledWith({
      phoneNumber: '0773964123',
      otp: '123456',
      newPassword: 'NewPassword9!'
    });

    expect(component.successMessage()).toContain(
      'successfully'
    );

    expect(component.isSubmitting()).toBe(false);
  });
});
