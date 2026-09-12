using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Data;

public static class EventCategorySeed
{
    private static readonly string[] CanonicalCategories =
    [
        "Sports",
        "Concert",
        "Cinema",
        "Conference",
        "Festival"
    ];

    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existingNames = await dbContext.EventCategories
            .AsNoTracking()
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

        var existing =
            new HashSet<string>(
                existingNames,
                StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;

        foreach (var categoryName in CanonicalCategories)
        {
            if (existing.Contains(categoryName))
            {
                continue;
            }

            dbContext.EventCategories.Add(
                new Entities.EventCategory
                {
                    Name = categoryName,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

            existing.Add(categoryName);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
