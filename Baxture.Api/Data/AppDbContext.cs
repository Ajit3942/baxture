using System.Text.Json;
using Baxture.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Baxture.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var hobbiesConverter = new ValueConverter<List<string>, string>(
            hobbies => JsonSerializer.Serialize(hobbies, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());

        var hobbiesComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            hobbies => hobbies.Aggregate(0, (hash, hobby) => HashCode.Combine(hash, hobby.GetHashCode())),
            hobbies => hobbies.ToList());

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasMaxLength(36);
            entity.Property(user => user.Username).IsRequired().HasMaxLength(100);
            entity.Property(user => user.Password).IsRequired().HasMaxLength(100);
            entity.Property(user => user.Age).IsRequired();
            entity.Property(user => user.IsAdmin).HasDefaultValue(false);
            entity.Property(user => user.Hobbies)
                .HasConversion(hobbiesConverter)
                .Metadata.SetValueComparer(hobbiesComparer);
        });
    }
}
