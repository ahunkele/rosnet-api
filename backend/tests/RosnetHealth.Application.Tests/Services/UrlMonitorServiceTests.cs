using Moq;
using RosnetHealth.Application.Dtos;
using RosnetHealth.Application.Exceptions;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Application.Mapping;
using RosnetHealth.Application.Services;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Application.Tests.Services;

public class UrlMonitorServiceTests
{
    private readonly Mock<IMonitoredUrlRepository> _urlRepository = new();
    private readonly Mock<IHealthCheckRepository> _healthCheckRepository = new();
    private readonly UrlMonitorService _sut;

    public UrlMonitorServiceTests()
    {
        _sut = new UrlMonitorService(_urlRepository.Object, _healthCheckRepository.Object, new UrlMapper());
    }

    [Fact]
    public async Task GetUrlsAsync_UrlWithNoChecks_MapsToNullStatus()
    {
        var checkedUrl = new MonitoredUrlEntity { Id = 1, Name = "Checked", Url = "https://checked.example" };
        var uncheckedUrl = new MonitoredUrlEntity { Id = 2, Name = "Unchecked", Url = "https://unchecked.example" };
        var latestCheck = HealthCheckEntity.Create(checkedUrl.Id, DateTime.UtcNow, 200, 50);

        _urlRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync([checkedUrl, uncheckedUrl]);
        _healthCheckRepository.Setup(r => r.GetLatestForAllAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([latestCheck]);

        var result = await _sut.GetUrlsAsync();

        var checkedDto = Assert.Single(result, dto => dto.Id == checkedUrl.Id);
        Assert.Equal(HealthStatus.Up, checkedDto.Status);

        var uncheckedDto = Assert.Single(result, dto => dto.Id == uncheckedUrl.Id);
        Assert.Null(uncheckedDto.Status);
        Assert.Null(uncheckedDto.LastCheckedAt);
    }

    [Fact]
    public async Task GetUrlsAsync_IncludesPausedUrls()
    {
        var pausedUrl = new MonitoredUrlEntity { Id = 1, Name = "Paused", Url = "https://paused.example", IsActive = false };

        _urlRepository.Setup(r => r.GetAllAsync()).ReturnsAsync([pausedUrl]);
        _healthCheckRepository.Setup(r => r.GetLatestForAllAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([]);

        var result = await _sut.GetUrlsAsync();

        var dto = Assert.Single(result);
        Assert.False(dto.IsActive);
    }

    [Fact]
    public async Task GetHistoryAsync_UrlNotFound_ReturnsNull()
    {
        _urlRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MonitoredUrlEntity?)null);

        var result = await _sut.GetHistoryAsync(urlId: 99);

        Assert.Null(result);
        _healthCheckRepository.Verify(r => r.GetHistoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetHistoryAsync_UrlFound_ReturnsMappedHistory()
    {
        var url = new MonitoredUrlEntity { Id = 1, Name = "Site", Url = "https://site.example" };
        var check = HealthCheckEntity.Create(url.Id, DateTime.UtcNow, 500, 100, "Internal Server Error");

        _urlRepository.Setup(r => r.GetByIdAsync(url.Id)).ReturnsAsync(url);
        _healthCheckRepository.Setup(r => r.GetHistoryAsync(url.Id, 50)).ReturnsAsync([check]);

        var result = await _sut.GetHistoryAsync(url.Id);

        var entry = Assert.Single(result!);
        Assert.Equal(HealthStatus.Down, entry.Status);
        Assert.Equal("Internal Server Error", entry.ErrorMessage);
    }

    [Fact]
    public async Task SetActiveAsync_UrlNotFound_ReturnsNullAndDoesNotUpdate()
    {
        _urlRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MonitoredUrlEntity?)null);

        var result = await _sut.SetActiveAsync(urlId: 99, isActive: false);

        Assert.Null(result);
        _urlRepository.Verify(r => r.UpdateAsync(It.IsAny<MonitoredUrlEntity>()), Times.Never);
    }

    [Fact]
    public async Task SetActiveAsync_UrlFound_UpdatesIsActive()
    {
        var url = new MonitoredUrlEntity { Id = 1, Name = "Site", Url = "https://site.example", IsActive = true };
        _urlRepository.Setup(r => r.GetByIdAsync(url.Id)).ReturnsAsync(url);
        _healthCheckRepository.Setup(r => r.GetLatestForAllAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([]);

        var result = await _sut.SetActiveAsync(url.Id, isActive: false);

        Assert.False(url.IsActive);
        Assert.False(result!.IsActive);
        _urlRepository.Verify(r => r.UpdateAsync(url), Times.Once);
    }

    [Fact]
    public async Task DeleteUrlAsync_UrlNotFound_ReturnsFalseAndDoesNotDelete()
    {
        _urlRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MonitoredUrlEntity?)null);

        var result = await _sut.DeleteUrlAsync(urlId: 99);

        Assert.False(result);
        _urlRepository.Verify(r => r.DeleteAsync(It.IsAny<MonitoredUrlEntity>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUrlAsync_UrlFound_DeletesAndReturnsTrue()
    {
        var url = new MonitoredUrlEntity { Id = 1, Name = "Site", Url = "https://site.example" };
        _urlRepository.Setup(r => r.GetByIdAsync(url.Id)).ReturnsAsync(url);

        var result = await _sut.DeleteUrlAsync(url.Id);

        Assert.True(result);
        _urlRepository.Verify(r => r.DeleteAsync(url), Times.Once);
    }

    [Fact]
    public async Task AddUrlAsync_DuplicateUrl_ThrowsDuplicateUrlException()
    {
        var request = new AddUrlRequest("Site", "https://site.example");
        _urlRepository.Setup(r => r.ExistsByUrlAsync(request.Url)).ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateUrlException>(() => _sut.AddUrlAsync(request));

        _urlRepository.Verify(r => r.AddAsync(It.IsAny<MonitoredUrlEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddUrlAsync_NewUrl_AddsAndReturnsDto()
    {
        var request = new AddUrlRequest("Site", "https://site.example");
        _urlRepository.Setup(r => r.ExistsByUrlAsync(request.Url)).ReturnsAsync(false);

        var result = await _sut.AddUrlAsync(request);

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Url, result.Url);
        Assert.Null(result.Status);
        _urlRepository.Verify(r => r.AddAsync(It.Is<MonitoredUrlEntity>(e => e.Url == request.Url)), Times.Once);
    }
}
