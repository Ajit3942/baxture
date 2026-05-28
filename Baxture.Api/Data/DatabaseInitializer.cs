using Baxture.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Baxture.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        dbContext.Users.AddRange(
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Password = "admin123",
                IsAdmin = true,
                Age = 35,
                Hobbies = ["architecture", "running"]
            },
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "user",
                Password = "user123",
                IsAdmin = false,
                Age = 28,
                Hobbies = ["reading"]
            });

        await dbContext.SaveChangesAsync();
    }
}
