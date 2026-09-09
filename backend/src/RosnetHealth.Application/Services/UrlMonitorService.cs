using RosnetHealth.Application.Dtos;
using RosnetHealth.Application.Exceptions;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Application.Mapping;
using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Application.Services;

public class UrlMonitorService(
    IMonitoredUrlRepository urlRepository,
    IHealthCheckRepository healthCheckRepository,
    UrlMapper mapper) : IUrlMonitorService
{
    public async Task<UrlStatusDto> AddUrlAsync(AddUrlRequest request)
    {
        if (await urlRepository.ExistsByUrlAsync(request.Url))
        {
            throw new DuplicateUrlException(request.Url);
        }

        var entity = new MonitoredUrlEntity
        {
            Name = request.Name,
            Url = request.Url,
            CreatedAt = DateTime.UtcNow
        };

        await urlRepository.AddAsync(entity);

        return mapper.ToStatusDto(entity, latestCheck: null);
    }

    public async Task<IReadOnlyList<UrlStatusDto>> GetUrlsAsync()
    {
        var urls = await urlRepository.GetAllAsync();
        var latestChecks = await healthCheckRepository.GetLatestForAllAsync(urls.Select(u => u.Id));
        var latestByUrlId = latestChecks.ToDictionary(c => c.MonitoredUrlId);

        return urls
            .Select(url => mapper.ToStatusDto(url, latestByUrlId.GetValueOrDefault(url.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<HealthCheckHistoryEntryDto>?> GetHistoryAsync(int urlId, int take = 50)
    {
        var url = await urlRepository.GetByIdAsync(urlId);
        if (url is null)
        {
            return null;
        }

        var history = await healthCheckRepository.GetHistoryAsync(urlId, take);
        return history.Select(mapper.ToHistoryDto).ToList();
    }

    public async Task<UrlStatusDto?> SetActiveAsync(int urlId, bool isActive)
    {
        var url = await urlRepository.GetByIdAsync(urlId);
        if (url is null)
        {
            return null;
        }

        url.IsActive = isActive;
        await urlRepository.UpdateAsync(url);

        var latestChecks = await healthCheckRepository.GetLatestForAllAsync([urlId]);
        return mapper.ToStatusDto(url, latestChecks.FirstOrDefault());
    }

    public async Task<bool> DeleteUrlAsync(int urlId)
    {
        var url = await urlRepository.GetByIdAsync(urlId);
        if (url is null)
        {
            return false;
        }

        await urlRepository.DeleteAsync(url);
        return true;
    }
}
