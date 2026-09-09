using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Infrastructure.Persistence.Configurations;

public class MonitoredUrlConfiguration : IEntityTypeConfiguration<MonitoredUrlEntity>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<MonitoredUrlEntity> builder)
    {
        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Url).IsRequired().HasMaxLength(2000);

        builder.HasIndex(u => u.Url).IsUnique();

        builder.HasMany(u => u.CheckResults)
            .WithOne(c => c.MonitoredUrl)
            .HasForeignKey(c => c.MonitoredUrlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new
            {
                Id = 1,
                Name = "GitHub",
                Url = "https://github.com",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new
            {
                Id = 2,
                Name = "Google",
                Url = "https://www.google.com",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new
            {
                Id = 3,
                Name = "Simulated Down Service",
                Url = "https://httpstat.us/500",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            });
    }
}
