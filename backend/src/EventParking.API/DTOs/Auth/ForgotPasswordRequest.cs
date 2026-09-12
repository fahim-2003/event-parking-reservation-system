using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Auth;

public sealed class ForgotPasswordRequest
{
    [Required]
    [RegularExpression(
        @"^\+?[0-9]{9,15}$",
        ErrorMessage = "Enter a valid phone number.")]
    public string PhoneNumber { get; init; } = string.Empty;
}
