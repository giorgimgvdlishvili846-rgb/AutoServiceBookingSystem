using AutoServiceBookingSystem.Models;
using AutoServiceBookingSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        if (!await context.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            var username = configuration["SeedAdmin:Username"] ?? "admin";
            var email = configuration["SeedAdmin:Email"] ?? "admin@autoservice.local";
            var password = configuration["SeedAdmin:Password"] ?? "Admin123!";

            context.Users.Add(new User
            {
                Username = username,
                Email = email,
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Admin
            });
        }

        if (!await context.Services.AnyAsync())
        {
            context.Services.AddRange(
                new Service
                {
                    Name = "Oil Change",
                    Description = "Engine oil and oil filter replacement.",
                    Price = 90,
                    DurationMinutes = 45
                },
                new Service
                {
                    Name = "Computer Diagnostics",
                    Description = "Full electronic system diagnostics.",
                    Price = 70,
                    DurationMinutes = 60
                },
                new Service
                {
                    Name = "Brake Inspection",
                    Description = "Brake pads, discs and fluid inspection.",
                    Price = 50,
                    DurationMinutes = 45
                });
        }

        await context.SaveChangesAsync();
    }
}
