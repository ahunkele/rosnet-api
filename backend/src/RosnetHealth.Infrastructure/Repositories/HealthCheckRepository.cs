using Microsoft.EntityFrameworkCore;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Infrastructure.Persistence;

namespace RosnetHealth.Infrastructure.Repositories;

public class HealthCheckRepository(AppDbContext context) : IHealthCheckRepository
{
    public async Task AddAsync(HealthCheckEntity entity)
    {
        context.HealthChecks.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<HealthCheckEntity>> GetHistoryAsync(int monitoredUrlId, int take = 50) =>
        await context.HealthChecks
            .Where(c => c.MonitoredUrlId == monitoredUrlId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(take)
            .ToListAsync();

    public async Task<IReadOnlyList<HealthCheckEntity>> GetLatestForAllAsync(IEnumerable<int> monitoredUrlIds) =>
        await context.HealthChecks
            .Where(c => monitoredUrlIds.Contains(c.MonitoredUrlId))
            .GroupBy(c => c.MonitoredUrlId)
            .Select(g => g.OrderByDescending(c => c.CheckedAt).First())
            .ToListAsync();
}
