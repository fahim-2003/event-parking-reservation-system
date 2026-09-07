using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Range(
        typeof(bool),
        "true",
        "true",
        ErrorMessage = "You must accept the terms and conditions.")]
    public bool AcceptedTerms { get; init; }
}