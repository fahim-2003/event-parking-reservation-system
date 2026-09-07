using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Auth;

public sealed class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;
}