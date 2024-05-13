using Microsoft.AspNetCore.Identity;
namespace Tourism
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // string adminEmail = "admin@gmail.com";
            // string password = "Qwerty_1";
            if (await roleManager.FindByNameAsync("user") == null)
            {
                await roleManager.CreateAsync(new IdentityRole<int>("user"));
            }

            if (await roleManager.FindByNameAsync("guide") == null)
            {
                await roleManager.CreateAsync(new IdentityRole<int>("guide"));
            }

            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole<int>("admin"));
            }

            
            // if (await userManager.FindByNameAsync(adminEmail) == null)
            // {
            //     User admin = new User { Email = adminEmail, UserName = adminEmail };
            //     IdentityResult result = await userManager.CreateAsync(admin, password);
            //     if (result.Succeeded)
            //     {
            //         await userManager.AddToRoleAsync(admin, "admin");
            //     }
            // }
        }
    }
}