using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            string[] roles = { "SuperAdmin", "Recruiter", "HiringManager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new AppRole { Name = role });
            }
            var departmentNames = new[] { "Engineering", "Product", "Finance", "Creative", "People", "Marketing" };
            foreach (var name in departmentNames)
            {
                var exists = await context.Departments.AnyAsync(d => d.Name == name);
                if (!exists)
                    context.Departments.Add(new Department { Name = name, Description = $"{name} department" });
            }
            await context.SaveChangesAsync();
            await SeedUserAsync(userManager, "admin@ctrecruita.com", "Admin@123", "System", "Admin", UserType.SuperAdmin, "SuperAdmin");
            await SeedUserAsync(userManager, "recruiter@mailinator.com", "Recruiter@123", "Lola", "Adebayo", UserType.Recruiter, "Recruiter");
            await SeedUserAsync(userManager, "hiringmanager@mailinator.com", "Manager@123", "Chidi", "Eze", UserType.HiringManager, "HiringManager");
        }
        private static async Task SeedUserAsync(
            UserManager<AppUser> userManager,
            string email,
            string password,
            string firstName,
            string lastName,
            UserType userType,
            string roleName)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
                return;
            var user = new AppUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                UserType = userType,
                UserStatus = UserStatus.Active,
                RequiresPasswordReset = false,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, roleName);
        }
    }
}