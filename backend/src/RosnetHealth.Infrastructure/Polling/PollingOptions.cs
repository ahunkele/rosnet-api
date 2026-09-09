namespace RosnetHealth.Infrastructure.Polling;

public class PollingOptions
{
    public const string SectionName = "HealthCheckPolling";

    public int IntervalSeconds { get; set; } = 30;
}
