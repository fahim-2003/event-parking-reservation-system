namespace EventParking.API.DTOs.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string UserId { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string AccountStatus { get; init; } = string.Empty;
}