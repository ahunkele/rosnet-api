using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Application.Interfaces;

public interface IHealthCheckRepository
{
    Task AddAsync(HealthCheckEntity entity);
    Task<IReadOnlyList<HealthCheckEntity>> GetHistoryAsync(int monitoredUrlId, int take = 50);
    Task<IReadOnlyList<HealthCheckEntity>> GetLatestForAllAsync(IEnumerable<int> monitoredUrlIds);
}
