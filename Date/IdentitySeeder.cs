using Microsoft.AspNetCore.Identity;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.Date;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roles = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { "Admin", "Driver" })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var roleResult = await roles.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ", roleResult.Errors.Select(error => error.Description)));
                }
            }
        }

        var userName = configuration["SeedAdmin:UserName"];
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Admin seed settings are missing from User Secrets.");
        }

        var admin = await users.FindByNameAsync(userName);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var createResult = await users.CreateAsync(admin, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", createResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await users.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await users.AddToRoleAsync(admin, "Admin");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            }
        }
    }
}