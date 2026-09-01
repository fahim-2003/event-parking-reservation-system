using EventParking.API.DTOs.Auth;
using EventParking.API.Services;

namespace EventParking.API.Interfaces.Services;

public interface IAuthService
{
    Task<AuthOperationResult<RegisterResponse>> RegisterCustomerAsync(
        RegisterRequest request);

    Task<AuthOperationResult<AuthResponse>> LoginAsync(
        LoginRequest request);
}