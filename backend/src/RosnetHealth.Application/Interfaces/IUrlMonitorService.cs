using RosnetHealth.Application.Dtos;

namespace RosnetHealth.Application.Interfaces;

public interface IUrlMonitorService
{
    Task<UrlStatusDto> AddUrlAsync(AddUrlRequest request);
    Task<IReadOnlyList<UrlStatusDto>> GetUrlsAsync();
    Task<IReadOnlyList<HealthCheckHistoryEntryDto>?> GetHistoryAsync(int urlId, int take = 50);
    Task<UrlStatusDto?> SetActiveAsync(int urlId, bool isActive);
    Task<bool> DeleteUrlAsync(int urlId);
}
