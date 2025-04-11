using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Models; // Required for ApplicationUser if seeding users later
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInventory.Data
{
    public static class SeedData
    {
        // Method to initialize roles
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Get RoleManager service
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define the roles to be created
            string[] roleNames = { "Admin", "Regular User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                // Check if the role already exists
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the role if it doesn't exist
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

                    // Log errors if role creation fails (optional but recommended)
                    if (!roleResult.Succeeded)
                    {
                         Console.WriteLine($"Error creating role {roleName}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // --- Seed a default Admin User ---
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string adminEmail = "parsamollahoseini7@gmail.com";
            string adminPassword = "Qwaszx12m."; // Use the provided strong password

            // Check if the admin user already exists
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                Console.WriteLine($"Creating default admin user: {adminEmail}");
                ApplicationUser adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin User", // Default Full Name
                    EmailConfirmed = true // Set EmailConfirmed to true to bypass email verification for this seeded user
                };

                IdentityResult userResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (userResult.Succeeded)
                {
                    Console.WriteLine("Default admin user created successfully.");
                    // Assign the 'Admin' role to the new user
                    // Ensure the 'Admin' role definitely exists first (it should from above)
                    if (await roleManager.RoleExistsAsync("Admin"))
                    {
                         IdentityResult roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                         if (roleAssignResult.Succeeded)
                         {
                             Console.WriteLine("Assigned 'Admin' role to default admin user.");
                         }
                         else
                         {
                             Console.WriteLine($"Error assigning 'Admin' role: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
                         }
                    }
                    else
                    {
                         Console.WriteLine("Error: 'Admin' role does not exist. Cannot assign role to default admin user.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error creating default admin user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                 Console.WriteLine($"Default admin user ({adminEmail}) already exists.");
            }
            // --- End Admin User Seed ---
        }
    }
}
