using Microsoft.Extensions.DependencyInjection;
using RosnetHealth.Application.Interfaces;
using RosnetHealth.Application.Mapping;
using RosnetHealth.Application.Services;

namespace RosnetHealth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUrlMonitorService, UrlMonitorService>();
        services.AddSingleton<UrlMapper>();

        return services;
    }
}
