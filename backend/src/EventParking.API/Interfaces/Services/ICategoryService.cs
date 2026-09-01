using EventParking.API.DTOs.Categories;

namespace EventParking.API.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CategoryResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}