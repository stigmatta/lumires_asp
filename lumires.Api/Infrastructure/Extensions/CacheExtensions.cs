using lumires.Api.Core.Options;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace lumires.Api.Infrastructure.Extensions;

internal static class CacheExtensions
{
    public static IServiceCollection AddLumiresCache(this IServiceCollection services, IConfiguration configuration)
    {
        var fusionConfig = configuration
            .GetSection(FusionCacheConfig.Section)
            .Get<FusionCacheConfig>() ?? new FusionCacheConfig();

        var redisConnectionString = configuration.GetConnectionString("cache")
                                    ?? throw new InvalidOperationException(
                                        "Redis connection string 'cache' not found.");

        services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });

        var fusionSerializer = new FusionCacheSystemTextJsonSerializer();

        services.AddFusionCache()
            .WithOptions(new FusionCacheOptions
            {
                DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    MemoryCacheDuration = TimeSpan.FromMinutes(fusionConfig.MemoryDurationMin),
                    Duration = TimeSpan.FromMinutes(fusionConfig.DistributedDurationMin),
                    FailSafeMaxDuration = TimeSpan.FromHours(fusionConfig.FailSafeMaxDurationHours),
                    FactorySoftTimeout = TimeSpan.FromMilliseconds(fusionConfig.FactoryTimeoutMs),
                    IsFailSafeEnabled = true
                },
                IncludeTagsInTraces = true,
                IncludeTagsInMetrics = true
            })
            .WithSerializer(fusionSerializer)
            .WithDistributedCache(
                sp => sp.GetRequiredService<IDistributedCache>(),
                fusionSerializer
            );

        return services;
    }
}