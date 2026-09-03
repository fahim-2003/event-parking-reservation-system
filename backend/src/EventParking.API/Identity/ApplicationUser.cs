using EventParking.API.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventParking.API.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}