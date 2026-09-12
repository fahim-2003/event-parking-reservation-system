using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Auth;

public sealed class ResetPasswordRequest
{
    [Required]
    [RegularExpression(
        @"^\+?[0-9]{9,15}$",
        ErrorMessage = "Enter a valid phone number.")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^[0-9]{6}$",
        ErrorMessage = "OTP must contain exactly 6 digits.")]
    public string Otp { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;
}
