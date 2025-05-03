using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ultraReader.Data
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Rolleri oluştur
            string[] roleNames = { "Admin", "Moderator" };
            
            foreach (var roleName in roleNames)
            {
                // Rol yoksa oluştur
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin kullanıcısı oluştur
            var adminUser = new IdentityUser
            {
                UserName = "admin@ultrareader.com",
                Email = "admin@ultrareader.com",
                EmailConfirmed = true
            };

            var adminExists = await userManager.FindByEmailAsync(adminUser.Email);
            
            if (adminExists == null)
            {
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Moderatör kullanıcısı oluştur
            var moderatorUser = new IdentityUser
            {
                UserName = "moderator@ultrareader.com",
                Email = "moderator@ultrareader.com",
                EmailConfirmed = true
            };

            var moderatorExists = await userManager.FindByEmailAsync(moderatorUser.Email);
            
            if (moderatorExists == null)
            {
                var result = await userManager.CreateAsync(moderatorUser, "Moderator123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderatorUser, "Moderator");
                }
            }
        }
    }
} 