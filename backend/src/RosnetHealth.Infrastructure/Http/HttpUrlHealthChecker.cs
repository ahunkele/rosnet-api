using System.Diagnostics;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Infrastructure.Http;

public class HttpUrlHealthChecker(HttpClient httpClient) : IUrlHealthChecker
{
    public async Task<HealthCheckEntity> CheckAsync(MonitoredUrlEntity url, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var checkedAt = DateTime.UtcNow;

        try
        {
            using var response = await httpClient.GetAsync(url.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            stopwatch.Stop();

            return HealthCheckEntity.Create(
                url.Id,
                checkedAt,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Real cancellation (e.g. app shutdown), not a request timeout - let it propagate.
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            stopwatch.Stop();

            return HealthCheckEntity.Create(
                url.Id,
                checkedAt,
                statusCode: null,
                stopwatch.ElapsedMilliseconds,
                errorMessage: ex.Message);
        }
    }
}
