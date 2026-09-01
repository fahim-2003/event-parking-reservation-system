using EventParking.API.DTOs.Auth;
using EventParking.API.Services;

namespace EventParking.API.Interfaces.Services;

public interface IAuthService
{
    Task<AuthOperationResult<RegisterResponse>>
        RegisterCustomerAsync(RegisterRequest request);

    Task<AuthOperationResult<AuthResponse>>
        LoginAsync(LoginRequest request);

    Task<AuthOperationResult<MessageResponse>>
        VerifyEmailAsync(
            string userId,
            string token);

    Task<AuthOperationResult<MessageResponse>>
        ResendVerificationAsync(
            ResendVerificationRequest request);

    Task<AuthOperationResult<MessageResponse>>
        ForgotPasswordAsync(
            ForgotPasswordRequest request);

    Task<AuthOperationResult<MessageResponse>>
        ResetPasswordAsync(
            ResetPasswordRequest request);
}