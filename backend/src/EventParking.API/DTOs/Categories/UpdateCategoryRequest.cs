using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Categories;

public sealed class UpdateCategoryRequest
{
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be empty or whitespace.")]
    public string Name { get; set; } = string.Empty;
}