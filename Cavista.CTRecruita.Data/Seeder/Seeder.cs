using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Cavista.CTRecruita.Data.Seeder
{
    public static class Seeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            string[] roles = { "SuperAdmin", "Recruiter", "HiringManager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new AppRole { Name = role });
            }

            const string adminEmail = "admin@ctrecruita.com";
            const string adminPassword = "Admin@123";
            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing is null)
            {
                var admin = new AppUser
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true,
                    UserType = UserType.SuperAdmin,
                    UserStatus = UserStatus.Active,
                    RequiresPasswordReset = false,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
        }
    }
}