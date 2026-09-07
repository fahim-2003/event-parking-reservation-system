namespace EventParking.API.DTOs.Customers;

public sealed class AdminCustomerSummaryResponse
{
    public string UserId { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public bool EmailVerified { get; init; }

    public string AccountStatus { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }
}