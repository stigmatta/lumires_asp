using Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Infrastructure.Extensions;

internal static class CacheExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var fusionConfig = configuration
            .GetSection(FusionCacheConfig.Section)
            .Get<FusionCacheConfig>() ?? new FusionCacheConfig();

        var redisConnectionString = configuration.GetConnectionString("cache");

        var serializer = new FusionCacheSystemTextJsonSerializer();


        var fusion = services.AddFusionCache()
            .WithOptions(new FusionCacheOptions
            {
                DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    MemoryCacheDuration = TimeSpan.FromMinutes(fusionConfig.MemoryDurationMin),
                    FailSafeMaxDuration = TimeSpan.FromHours(fusionConfig.FailSafeMaxDurationHours),
                    FactorySoftTimeout = TimeSpan.FromMilliseconds(fusionConfig.FactoryTimeoutMs),
                    IsFailSafeEnabled = true
                }
            })
            .WithSerializer(serializer);

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(o =>
                o.Configuration = redisConnectionString);

            fusion.WithDistributedCache(
                sp => sp.GetRequiredService<IDistributedCache>(),
                serializer
            );
        }

        return services;
    }
}