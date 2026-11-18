using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Seeding;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(AppRoles.Admin));
        }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                DisplayName = configuration["SeedAdmin:DisplayName"] ?? "Store Admin",
                Email = email,
                UserName = email,
            };

            var creation = await userManager.CreateAsync(admin, password);
            if (!creation.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user: {string.Join(" ", creation.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
