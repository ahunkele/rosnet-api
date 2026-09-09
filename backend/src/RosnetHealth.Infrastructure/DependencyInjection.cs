using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Infrastructure.Http;
using RosnetHealth.Infrastructure.Persistence;
using RosnetHealth.Infrastructure.Polling;
using RosnetHealth.Infrastructure.Repositories;

namespace RosnetHealth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IMonitoredUrlRepository, MonitoredUrlRepository>();
        services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();

        services.AddHttpClient<IUrlHealthChecker, HttpUrlHealthChecker>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        var pollingOptions = configuration.GetSection(PollingOptions.SectionName).Get<PollingOptions>()
            ?? new PollingOptions();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(HealthCheckJob));
            q.AddJob<HealthCheckJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(HealthCheckJob)}-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(pollingOptions.IntervalSeconds)
                    .RepeatForever()));
        });
        services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return services;
    }
}
