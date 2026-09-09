using Microsoft.EntityFrameworkCore;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Infrastructure.Persistence;

namespace RosnetHealth.Infrastructure.Repositories;

public class MonitoredUrlRepository(AppDbContext context) : IMonitoredUrlRepository
{
    public Task<MonitoredUrlEntity?> GetByIdAsync(int id) =>
        context.MonitoredUrls.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<MonitoredUrlEntity>> GetAllActiveAsync() =>
        await context.MonitoredUrls.Where(u => u.IsActive).ToListAsync();

    public Task<bool> ExistsByUrlAsync(string url) =>
        context.MonitoredUrls.AnyAsync(u => u.Url == url);

    public async Task AddAsync(MonitoredUrlEntity entity)
    {
        context.MonitoredUrls.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MonitoredUrlEntity entity)
    {
        context.MonitoredUrls.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MonitoredUrlEntity entity)
    {
        context.MonitoredUrls.Remove(entity);
        await context.SaveChangesAsync();
    }
}
