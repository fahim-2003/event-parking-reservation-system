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
                throw new InvalidOperationException(
                    $"Unable to seed role '{roleName}'.");
            }
        }

        var adminEmail = adminOptions.Email.Trim();

        var administrator =
            await userManager.FindByEmailAsync(adminEmail);

        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminOptions.FullName.Trim(),
                AccountStatus = AccountStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            var result =
                await userManager.CreateAsync(
                    administrator,
                    adminOptions.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to create administrator.");
            }
        }

        if (!await userManager.IsInRoleAsync(
                administrator,
                AppRoles.Administrator))
        {
            await userManager.AddToRoleAsync(
                administrator,
                AppRoles.Administrator);
        }


        var customerEmail = "customer@gmail.com";

        var customer =
            await userManager.FindByEmailAsync(customerEmail);

        if (customer is null)
        {
            customer = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                EmailConfirmed = true,
                FullName = "Demo Customer",
                AccountStatus = AccountStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            var customerResult =
                await userManager.CreateAsync(
                    customer,
                    "Customer@123");

            if (!customerResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to create customer.");
            }
        }

        if (!await userManager.IsInRoleAsync(
                customer,
                AppRoles.Customer))
        {
            await userManager.AddToRoleAsync(
                customer,
                AppRoles.Customer);
        }
    }
}
