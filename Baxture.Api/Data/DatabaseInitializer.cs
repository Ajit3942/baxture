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
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('Users', 'CreatedBy') IS NULL
                ALTER TABLE [Users] ADD [CreatedBy] nvarchar(100) NOT NULL CONSTRAINT [DF_Users_CreatedBy] DEFAULT N'system';

            IF COL_LENGTH('Users', 'CreatedDate') IS NULL
                ALTER TABLE [Users] ADD [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_Users_CreatedDate] DEFAULT SYSUTCDATETIME();

            IF COL_LENGTH('Users', 'UpdatedBy') IS NULL
                ALTER TABLE [Users] ADD [UpdatedBy] nvarchar(100) NULL;

            IF COL_LENGTH('Users', 'UpdatedDate') IS NULL
                ALTER TABLE [Users] ADD [UpdatedDate] datetime2 NULL;
            """);

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var seedDate = DateTime.UtcNow;
        dbContext.Users.AddRange(
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Password = "admin123",
                IsAdmin = true,
                Age = 35,
                Hobbies = ["architecture", "running"],
                CreatedBy = "system",
                CreatedDate = seedDate
            },
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "user",
                Password = "user123",
                IsAdmin = false,
                Age = 28,
                Hobbies = ["reading"],
                CreatedBy = "system",
                CreatedDate = seedDate
            });

        await dbContext.SaveChangesAsync();
    }
}
