using JetBrains.Annotations;

namespace Core.Models;

[UsedImplicitly]
public record MovieSource(
    int ExternalId,
    string ProviderName,
    string Type,
    Uri Url,
    string Quality,
    double? Price
);