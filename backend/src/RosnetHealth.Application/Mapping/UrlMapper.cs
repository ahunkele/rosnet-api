using Riok.Mapperly.Abstractions;
using RosnetHealth.Application.Dtos;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Application.Mapping;

[Mapper]
public partial class UrlMapper
{
    [MapperIgnoreSource(nameof(HealthCheckEntity.Id))]
    [MapperIgnoreSource(nameof(HealthCheckEntity.MonitoredUrlId))]
    [MapperIgnoreSource(nameof(HealthCheckEntity.MonitoredUrl))]
    public partial HealthCheckHistoryEntryDto ToHistoryDto(HealthCheckEntity entity);

    // Composes fields from two entities, so it's hand-written rather than Mapperly-generated.
    public UrlStatusDto ToStatusDto(MonitoredUrlEntity url, HealthCheckEntity? latestCheck)
    {
        return new UrlStatusDto(
            url.Id,
            url.Name,
            url.Url,
            url.IsActive,
            latestCheck?.Status,
            latestCheck?.StatusCode,
            latestCheck?.ResponseTimeMs,
            latestCheck?.CheckedAt,
            latestCheck?.ErrorMessage);
    }
}
