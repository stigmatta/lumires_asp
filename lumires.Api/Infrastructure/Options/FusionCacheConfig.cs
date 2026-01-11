namespace lumires.Api.Infrastructure.Options;

public class FusionCacheConfig
{
    public int MemoryDurationMin { get; set; }
    public int DistributedDurationMin { get; set; }
    public int FailSafeMaxDurationHours { get; set; }
    public int FactoryTimeoutMs { get; set; }
}