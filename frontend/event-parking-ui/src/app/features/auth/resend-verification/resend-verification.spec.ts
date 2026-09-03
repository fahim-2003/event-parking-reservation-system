import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';

import { ResendVerification } from './resend-verification';
import { AuthService } from '../../../core/services/auth.service';

describe('ResendVerification', () => {
  let fixture: ComponentFixture<ResendVerification>;
  let component: ResendVerification;

  const authServiceMock = {
    resendVerification: vi.fn()
  };

  beforeEach(async () => {
    authServiceMock.resendVerification.mockReset();

    await TestBed.configureTestingModule({
      imports: [ResendVerification],
      providers: [
        {
          provide: AuthService,
          useValue: authServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ResendVerification);
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
      authServiceMock.resendVerification
    ).not.toHaveBeenCalled();
  });

  it('should submit a valid email', () => {
    authServiceMock.resendVerification.mockReturnValue(
      of({
        message:
          'If verification is required, a verification email has been sent.'
      })
    );

    component.form.setValue({
      email: 'customer1@eventparking.local'
    });

    component.submit();

    expect(
      authServiceMock.resendVerification
    ).toHaveBeenCalledWith({
      email: 'customer1@eventparking.local'
    });

    expect(component.successMessage().length).toBeGreaterThan(0);
    expect(component.isSubmitting()).toBe(false);
  });
});
