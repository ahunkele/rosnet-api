namespace RosnetHealth.Domain.Entities;

public class MonitoredUrlEntity : Entity
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<HealthCheckEntity> CheckResults { get; set; } = new List<HealthCheckEntity>();
}
