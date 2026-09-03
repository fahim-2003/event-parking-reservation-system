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

  it('should reject an incomplete reset link', async () => {
    await createComponent();

    expect(component.hasValidLink).toBe(false);

    component.submit();

    expect(
      authServiceMock.resetPassword
    ).not.toHaveBeenCalled();

    expect(component.errorMessage()).toContain(
      'invalid'
    );
  });

  it('should reject mismatched passwords', async () => {
    queryParameters.set('userId', 'customer-1');
    queryParameters.set('token', 'reset-token');

    await createComponent();

    component.form.setValue({
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
    queryParameters.set('userId', 'customer-1');
    queryParameters.set('token', 'reset-token');

    authServiceMock.resetPassword.mockReturnValue(
      of({
        message: 'Password reset successfully.'
      })
    );

    await createComponent();

    component.form.setValue({
      newPassword: 'NewPassword9!',
      confirmPassword: 'NewPassword9!'
    });

    component.submit();

    expect(
      authServiceMock.resetPassword
    ).toHaveBeenCalledWith({
      userId: 'customer-1',
      token: 'reset-token',
      newPassword: 'NewPassword9!'
    });

    expect(component.successMessage()).toContain(
      'successfully'
    );

    expect(component.isSubmitting()).toBe(false);
  });
});
