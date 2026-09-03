using EventParking.API.Configurations;
using EventParking.API.Enums;
using EventParking.API.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EventParking.API.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminOptions =
            scope.ServiceProvider
                .GetRequiredService<IOptions<AdminSeedOptions>>()
                .Value;

        foreach (var roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Unable to seed role '{roleName}': {errors}");
            }
        }

        if (string.IsNullOrWhiteSpace(adminOptions.Email))
        {
            throw new InvalidOperationException(
                "AdminSeed:Email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(adminOptions.Password))
        {
            throw new InvalidOperationException(
                "AdminSeed:Password is not configured.");
        }

        if (string.IsNullOrWhiteSpace(adminOptions.FullName))
        {
            throw new InvalidOperationException(
                "AdminSeed:FullName is not configured.");
        }

        var adminEmail = adminOptions.Email.Trim();

        var administrator =
            await userManager.FindByEmailAsync(adminEmail);

        if (administrator is null)
        {
            var now = DateTime.UtcNow;

            administrator = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminOptions.FullName.Trim(),
                AccountStatus = AccountStatus.Active,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            var createResult =
                await userManager.CreateAsync(
                    administrator,
                    adminOptions.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(
                        error => error.Description));

                throw new InvalidOperationException(
                    $"Unable to seed Administrator account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                administrator,
                AppRoles.Administrator))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    administrator,
                    AppRoles.Administrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(
                        error => error.Description));

                throw new InvalidOperationException(
                    $"Unable to assign Administrator role: {errors}");
            }
        }
    }
}