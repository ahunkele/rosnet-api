using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Infrastructure.Persistence;
using RosnetHealth.Infrastructure.Repositories;

namespace RosnetHealth.Infrastructure.Tests.Repositories;

public class HealthCheckRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly HealthCheckRepository _sut;

    public HealthCheckRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _context.MonitoredUrls.RemoveRange(_context.MonitoredUrls);
        _context.SaveChanges();

        _sut = new HealthCheckRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<MonitoredUrlEntity> AddUrlAsync(string name)
    {
        var url = new MonitoredUrlEntity { Name = name, Url = $"https://{name}.example" };
        _context.MonitoredUrls.Add(url);
        await _context.SaveChangesAsync();
        return url;
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsNewestFirstAndRespectsTake()
    {
        var url = await AddUrlAsync("site");
        var now = DateTime.UtcNow;

        await _sut.AddAsync(HealthCheckEntity.Create(url.Id, now.AddMinutes(-2), 200, 10));
        await _sut.AddAsync(HealthCheckEntity.Create(url.Id, now.AddMinutes(-1), 200, 20));
        await _sut.AddAsync(HealthCheckEntity.Create(url.Id, now, 500, 30));

        var history = await _sut.GetHistoryAsync(url.Id, take: 2);

        Assert.Equal(2, history.Count);
        Assert.Equal(500, history[0].StatusCode); // newest first
        Assert.Equal(200, history[1].StatusCode);
    }

    [Fact]
    public async Task GetLatestForAllAsync_ReturnsOneEntryPerUrl_MostRecentOnly()
    {
        var urlA = await AddUrlAsync("a");
        var urlB = await AddUrlAsync("b");
        var now = DateTime.UtcNow;

        await _sut.AddAsync(HealthCheckEntity.Create(urlA.Id, now.AddMinutes(-5), 200, 10));
        await _sut.AddAsync(HealthCheckEntity.Create(urlA.Id, now, 500, 20)); // latest for A
        await _sut.AddAsync(HealthCheckEntity.Create(urlB.Id, now.AddMinutes(-1), 200, 30)); // latest for B

        var latest = await _sut.GetLatestForAllAsync([urlA.Id, urlB.Id]);

        Assert.Equal(2, latest.Count);
        Assert.Equal(500, latest.Single(c => c.MonitoredUrlId == urlA.Id).StatusCode);
        Assert.Equal(200, latest.Single(c => c.MonitoredUrlId == urlB.Id).StatusCode);
    }

    [Fact]
    public async Task GetLatestForAllAsync_UrlWithNoChecks_IsExcludedNotNull()
    {
        var url = await AddUrlAsync("unchecked");

        var latest = await _sut.GetLatestForAllAsync([url.Id]);

        Assert.Empty(latest);
    }
}
