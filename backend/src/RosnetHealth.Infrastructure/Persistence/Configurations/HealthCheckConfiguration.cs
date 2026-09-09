using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Infrastructure.Persistence.Configurations;

public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheckEntity>
{
    public void Configure(EntityTypeBuilder<HealthCheckEntity> builder)
    {
        builder.Property(c => c.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(c => new { c.MonitoredUrlId, c.CheckedAt });
    }
}
