namespace Infrastructure.Options;

internal class FusionCacheConfig
{
    public const string Section = "CacheSettings";
    public int MemoryDurationMin { get; init; }
    public int FailSafeMaxDurationHours { get; init; }
    public int FactoryTimeoutMs { get; init; }
}