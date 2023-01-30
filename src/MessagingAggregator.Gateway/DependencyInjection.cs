using MessagingAggregator.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingAggregator.Gateway;

public static class DependencyInjection
{
    public static IServiceCollection AddGateway(this IServiceCollection services)
    {
        services.AddScoped<IProvider, Provider>();
        services.AddHttpClient();

        return services;
    }
}