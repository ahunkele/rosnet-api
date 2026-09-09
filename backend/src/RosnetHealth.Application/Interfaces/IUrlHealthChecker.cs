using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Application.Interfaces;

public interface IUrlHealthChecker
{
    Task<HealthCheckEntity> CheckAsync(MonitoredUrlEntity url, CancellationToken cancellationToken);
}
