using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Domain.Entities;

public class HealthCheckEntity : Entity
{
    public int MonitoredUrlId { get; set; }
    public MonitoredUrlEntity? MonitoredUrl { get; set; }

    public DateTime CheckedAt { get; set; }
    public int? StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public HealthStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    public static HealthCheckEntity Create(
        int monitoredUrlId,
        DateTime checkedAt,
        int? statusCode,
        long responseTimeMs,
        string? errorMessage = null)
    {
        return new HealthCheckEntity
        {
            MonitoredUrlId = monitoredUrlId,
            CheckedAt = checkedAt,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            ErrorMessage = errorMessage,
            Status = DetermineStatus(statusCode)
        };
    }

    private const int SuccessRangeStart = 200;
    private const int SuccessRangeEnd = 399;

    private static HealthStatus DetermineStatus(int? statusCode)
    {
        if (statusCode is null)
        {
            return HealthStatus.Down; // no response at all: timeout, DNS failure, connection refused, etc.
        }

        return statusCode is >= SuccessRangeStart and <= SuccessRangeEnd
            ? HealthStatus.Up
            : HealthStatus.Down;
    }
}
