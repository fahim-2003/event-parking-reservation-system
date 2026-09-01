namespace EventParking.API.DTOs.Auth;

public sealed class RegisterResponse
{
    public string UserId { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public bool EmailVerificationRequired { get; init; }
}