using Microsoft.AspNetCore.Identity;

namespace PROG7311_GLMS_ST10435542.Data
{
// According to Microsoft (n.d.), role-based authorization is a powerful way to restrict access to certain parts of an application based on the user's role.
// In this implementation, I created two roles, "Admin" and "Staff". The DbInitializer class is responsible for seeding these roles and creating default users for each role when the application starts.
    public static class DbInitializer // this class helps fill the database with the default admin and staff profiles when the application starts
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider) // this method sets up the roles and the first users automatically
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "Staff", "Client", "Driver" }; // the three profiles uses

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role)) // it checks if the role already exists in the database
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync("admin@glms.com") == null) // creates the profile for admin
            {
                var admin = new IdentityUser { UserName = "admin@glms.com", Email = "admin@glms.com" };
                await userManager.CreateAsync(admin, "admin123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (await userManager.FindByEmailAsync("staff@glms.com") == null) // creates the profile for staff
            {
                var staff = new IdentityUser { UserName = "staff@glms.com", Email = "staff@glms.com" };
                await userManager.CreateAsync(staff, "staff123");
                await userManager.AddToRoleAsync(staff, "Staff");
            }

            if (await userManager.FindByEmailAsync("driver@glms.com") == null) // creates the profile for driver
            {
                var driver = new IdentityUser { UserName = "driver@glms.com", Email = "driver@glms.com" };
                await userManager.CreateAsync(driver, "driver123");
                await userManager.AddToRoleAsync(driver, "Driver");
            }
        }
    }
}

/* Reference List:
 
    Microsoft Docs. (n.d.). Role-based authorization in ASP.NET Core. [online]
    Available at: <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0>
    [Accessed 18 April 2026].

 */