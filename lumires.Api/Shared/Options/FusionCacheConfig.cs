namespace lumires.Api.Shared.Options;

public class FusionCacheConfig
{
    public const string Section = "CacheSettings";
    public int MemoryDurationMin { get; set; }
    public int DistributedDurationMin { get; set; }
    public int FailSafeMaxDurationHours { get; set; }
    public int FactoryTimeoutMs { get; set; }
}