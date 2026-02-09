using CreditFlowAPI.Base.Identity;
using Microsoft.AspNetCore.Identity;

namespace CreditFlowAPI.Base.Persistance
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndBankerAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roleNames = { "Admin", "Banker", "User" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                var bankerEmail = "banker@creditflow.com";
                var bankerUser = await userManager.FindByEmailAsync(bankerEmail);

                if (bankerUser == null)
                {
                    var newBanker = new ApplicationUser
                    {
                        UserName = bankerEmail,
                        Email = bankerEmail,
                        FirstName = "Mr.",
                        LastName = "Banker",
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(newBanker, "Banker123!");

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newBanker, "Banker");
                    }
                }
            }
        }
    }
}
