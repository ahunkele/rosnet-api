using RosnetHealth.Domain.Entities;

namespace RosnetHealth.Application.Interfaces;

public interface IMonitoredUrlRepository
{
    Task<MonitoredUrlEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<MonitoredUrlEntity>> GetAllAsync();
    Task<IReadOnlyList<MonitoredUrlEntity>> GetAllActiveAsync();
    Task<bool> ExistsByUrlAsync(string url);
    Task AddAsync(MonitoredUrlEntity entity);
    Task UpdateAsync(MonitoredUrlEntity entity);
    Task DeleteAsync(MonitoredUrlEntity entity);
}
