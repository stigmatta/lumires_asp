using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public record MovieSource(
    int ExternalId,
    string ProviderName,
    string Type,
    Uri Url,
    string Quality,
    double? Price
);