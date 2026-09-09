using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Infrastructure.Persistence;
using RosnetHealth.Infrastructure.Repositories;

namespace RosnetHealth.Infrastructure.Tests.Repositories;

public class MonitoredUrlRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly MonitoredUrlRepository _sut;

    public MonitoredUrlRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        // EnsureCreated() also applies HasData seed rows - clear them for a clean slate per test.
        _context.MonitoredUrls.RemoveRange(_context.MonitoredUrls);
        _context.SaveChanges();

        _sut = new MonitoredUrlRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var entity = new MonitoredUrlEntity { Name = "Site", Url = "https://site.example" };

        await _sut.AddAsync(entity);

        var found = await _sut.GetByIdAsync(entity.Id);
        Assert.NotNull(found);
        Assert.Equal("Site", found!.Name);
    }

    [Fact]
    public async Task GetAllAsync_IncludesActiveAndInactiveUrls()
    {
        await _sut.AddAsync(new MonitoredUrlEntity { Name = "Active", Url = "https://active.example", IsActive = true });
        await _sut.AddAsync(new MonitoredUrlEntity { Name = "Paused", Url = "https://paused.example", IsActive = false });

        var all = await _sut.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetAllActiveAsync_ExcludesInactiveUrls()
    {
        await _sut.AddAsync(new MonitoredUrlEntity { Name = "Active", Url = "https://active.example", IsActive = true });
        await _sut.AddAsync(new MonitoredUrlEntity { Name = "Paused", Url = "https://paused.example", IsActive = false });

        var active = await _sut.GetAllActiveAsync();

        var result = Assert.Single(active);
        Assert.Equal("Active", result.Name);
    }

    [Theory]
    [InlineData("https://exists.example", true)]
    [InlineData("https://missing.example", false)]
    public async Task ExistsByUrlAsync_ReflectsWhetherUrlIsPresent(string url, bool expected)
    {
        await _sut.AddAsync(new MonitoredUrlEntity { Name = "Site", Url = "https://exists.example" });

        var exists = await _sut.ExistsByUrlAsync(url);

        Assert.Equal(expected, exists);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangesAcrossSessions()
    {
        var entity = new MonitoredUrlEntity { Name = "Site", Url = "https://site.example", IsActive = true };
        await _sut.AddAsync(entity);

        entity.IsActive = false;
        await _sut.UpdateAsync(entity);

        // Verify against a second, independent context sharing the same in-memory DB,
        // so this isn't just reading back the first context's change tracker.
        await using var verifyContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        var reloaded = await verifyContext.MonitoredUrls.FindAsync(entity.Id);

        Assert.False(reloaded!.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_CascadesToHealthChecks()
    {
        var entity = new MonitoredUrlEntity { Name = "Site", Url = "https://site.example" };
        await _sut.AddAsync(entity);

        _context.HealthChecks.Add(HealthCheckEntity.Create(entity.Id, DateTime.UtcNow, 200, 50));
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(entity);

        var remainingChecks = await _context.HealthChecks.Where(c => c.MonitoredUrlId == entity.Id).ToListAsync();
        Assert.Empty(remainingChecks);
    }
}
