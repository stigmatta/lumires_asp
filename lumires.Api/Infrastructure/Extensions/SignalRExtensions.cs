using lumires.Api.Infrastructure.Hubs;
using lumires.Api.Options;

namespace lumires.Api.Infrastructure.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddLumiresSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var signalRConfig = configuration
            .GetSection("SignalRSettings")
            .Get<SignalRConfig>() ?? new SignalRConfig();

        var redisConnectionString = configuration.GetConnectionString("cache")
                                    ?? throw new InvalidOperationException(
                                        "Redis connection string 'cache' not found for SignalR.");

        services.AddSignalR()
            .AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.ConnectRetry = signalRConfig.RedisConnectRetry;
                options.Configuration.ConnectTimeout = signalRConfig.RedisConnectTimeoutMs;
                options.Configuration.AbortOnConnectFail = false;
            });

        return services;
    }
    
    public static void MapLumiresHubs(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var hubUrl = configuration["SignalRSettings:HubUrl"] ?? "/hubs/notifications";
        endpoints.MapHub<NotificationHub>(hubUrl);
    }
}