using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Infrastructure.Polling;

public class HealthCheckJob(
    IServiceScopeFactory scopeFactory,
    ILogger<HealthCheckJob> logger) : IJob
{
    public Task Execute(IJobExecutionContext context) => RunCheckCycleAsync(context.CancellationToken);

    private async Task RunCheckCycleAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MonitoredUrlEntity> urls;
        using (var scope = scopeFactory.CreateScope())
        {
            var urlRepository = scope.ServiceProvider.GetRequiredService<IMonitoredUrlRepository>();
            try
            {
                urls = await urlRepository.GetAllActiveAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load monitored URLs for health check cycle.");
                return;
            }
        }

        // Each concurrent check gets its own DI scope (and therefore its own DbContext),
        // since DbContext isn't thread-safe for concurrent operations on one instance.
        await Task.WhenAll(urls.Select(url => CheckAndRecordAsync(url, cancellationToken)));
    }

    private async Task CheckAndRecordAsync(MonitoredUrlEntity url, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var healthChecker = scope.ServiceProvider.GetRequiredService<IUrlHealthChecker>();
        var healthCheckRepository = scope.ServiceProvider.GetRequiredService<IHealthCheckRepository>();

        try
        {
            var result = await healthChecker.CheckAsync(url, cancellationToken);
            await healthCheckRepository.AddAsync(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // allow graceful shutdown to propagate
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Health check failed unexpectedly for {Url}", url.Url);
        }
    }
}
