import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { VerifyEmail } from './verify-email';
import { AuthService } from '../../../core/services/auth.service';

describe('VerifyEmail', () => {
  let fixture: ComponentFixture<VerifyEmail>;
  let component: VerifyEmail;

  const authServiceMock = {
    verifyEmail: vi.fn()
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
    authServiceMock.verifyEmail.mockReset();
    queryParameters.clear();
  });

  async function createComponent(): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [VerifyEmail],
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

    fixture = TestBed.createComponent(VerifyEmail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should reject an incomplete verification link', async () => {
    await createComponent();

    expect(authServiceMock.verifyEmail).not.toHaveBeenCalled();
    expect(component.isVerified()).toBe(false);
    expect(component.message()).toContain('invalid');
  });

  it('should verify a valid email link', async () => {
    queryParameters.set('userId', 'customer-1');
    queryParameters.set('token', 'verification-token');

    authServiceMock.verifyEmail.mockReturnValue(
      of({
        message: 'Email address verified successfully.'
      })
    );

    await createComponent();

    expect(authServiceMock.verifyEmail).toHaveBeenCalledWith(
      'customer-1',
      'verification-token'
    );

    expect(component.isVerified()).toBe(true);
    expect(component.message()).toContain('successfully');
  });

  it('should show verification failure', async () => {
    queryParameters.set('userId', 'customer-1');
    queryParameters.set('token', 'invalid-token');

    authServiceMock.verifyEmail.mockReturnValue(
      throwError(() => ({
        error: {
          detail: 'Invalid verification token.'
        }
      }))
    );

    await createComponent();

    expect(component.isVerified()).toBe(false);
    expect(component.message()).toBe(
      'Invalid verification token.'
    );
  });
});
