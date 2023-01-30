using MessagingAggregator.HostedService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingAggregator.HostedService;

public static class DependencyInjection
{
    public static IServiceCollection AddHostedService(this IServiceCollection services)
    {
        services.AddHostedService<ConsumerService>();

        return services;
    }
}