using Microsoft.EntityFrameworkCore;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MonitoredUrlEntity> MonitoredUrls => Set<MonitoredUrlEntity>();
    public DbSet<HealthCheckEntity> HealthChecks => Set<HealthCheckEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
