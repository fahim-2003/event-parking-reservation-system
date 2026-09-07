using EventParking.API.Data;
using EventParking.API.DTOs.Categories;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAtUtc = category.CreatedAtUtc,
                UpdatedAtUtc = category.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventCategories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAtUtc = category.CreatedAtUtc,
                UpdatedAtUtc = category.UpdatedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var category = new EventCategory
        {
            Name = request.Name.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.EventCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.EventCategories
            .SingleOrDefaultAsync(
                category => category.Id == id,
                cancellationToken);

        if (category is null)
        {
            return null;
        }

        category.Name = request.Name.Trim();
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(category);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.EventCategories
            .SingleOrDefaultAsync(
                category => category.Id == id,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        _dbContext.EventCategories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CategoryResponse ToResponse(EventCategory category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAtUtc = category.CreatedAtUtc,
            UpdatedAtUtc = category.UpdatedAtUtc
        };
    }
}