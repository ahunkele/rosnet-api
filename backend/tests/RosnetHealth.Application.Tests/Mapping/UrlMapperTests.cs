using RosnetHealth.Application.Mapping;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Application.Tests.Mapping;

public class UrlMapperTests
{
    private readonly UrlMapper _mapper = new();

    [Fact]
    public void ToStatusDto_NullLatestCheck_MapsLastCheckFieldsToNull()
    {
        var url = new MonitoredUrlEntity { Id = 1, Name = "Site", Url = "https://site.example", IsActive = true };

        var dto = _mapper.ToStatusDto(url, latestCheck: null);

        Assert.Equal(url.Id, dto.Id);
        Assert.Null(dto.Status);
        Assert.Null(dto.LastStatusCode);
        Assert.Null(dto.LastResponseTimeMs);
        Assert.Null(dto.LastCheckedAt);
        Assert.Null(dto.LastErrorMessage);
    }

    [Fact]
    public void ToStatusDto_WithLatestCheck_MapsFieldsFromCheck()
    {
        var url = new MonitoredUrlEntity { Id = 1, Name = "Site", Url = "https://site.example", IsActive = true };
        var check = HealthCheckEntity.Create(url.Id, DateTime.UtcNow, 200, 42);

        var dto = _mapper.ToStatusDto(url, check);

        Assert.Equal(HealthStatus.Up, dto.Status);
        Assert.Equal(200, dto.LastStatusCode);
        Assert.Equal(42, dto.LastResponseTimeMs);
        Assert.Equal(check.CheckedAt, dto.LastCheckedAt);
    }

    [Fact]
    public void ToHistoryDto_MapsFieldsFromEntity()
    {
        var check = HealthCheckEntity.Create(monitoredUrlId: 1, DateTime.UtcNow, 503, 99, "Service Unavailable");

        var dto = _mapper.ToHistoryDto(check);

        Assert.Equal(check.CheckedAt, dto.CheckedAt);
        Assert.Equal(503, dto.StatusCode);
        Assert.Equal(99, dto.ResponseTimeMs);
        Assert.Equal(HealthStatus.Down, dto.Status);
        Assert.Equal("Service Unavailable", dto.ErrorMessage);
    }
}
