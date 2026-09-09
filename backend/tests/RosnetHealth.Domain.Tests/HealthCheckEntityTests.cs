using RosnetHealth.Domain.Entities;
using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Domain.Tests;

public class HealthCheckEntityTests
{
    [Theory]
    [InlineData(null, HealthStatus.Down)] // no response at all: timeout, DNS failure, connection refused
    [InlineData(199, HealthStatus.Down)]
    [InlineData(200, HealthStatus.Up)]
    [InlineData(399, HealthStatus.Up)]
    [InlineData(400, HealthStatus.Down)]
    [InlineData(500, HealthStatus.Down)]
    public void Create_ClassifiesStatusFromStatusCode(int? statusCode, HealthStatus expectedStatus)
    {
        var result = HealthCheckEntity.Create(
            monitoredUrlId: 1,
            checkedAt: DateTime.UtcNow,
            statusCode: statusCode,
            responseTimeMs: 42);

        Assert.Equal(expectedStatus, result.Status);
    }

    [Fact]
    public void Create_PreservesInputValues()
    {
        var checkedAt = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc);

        var result = HealthCheckEntity.Create(
            monitoredUrlId: 7,
            checkedAt: checkedAt,
            statusCode: 503,
            responseTimeMs: 1234,
            errorMessage: "Service Unavailable");

        Assert.Equal(7, result.MonitoredUrlId);
        Assert.Equal(checkedAt, result.CheckedAt);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal(1234, result.ResponseTimeMs);
        Assert.Equal("Service Unavailable", result.ErrorMessage);
    }
}
