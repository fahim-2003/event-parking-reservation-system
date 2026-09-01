using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Customers;

public sealed class UpdateCustomerProfileRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;
}