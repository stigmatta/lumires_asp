using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public record FilmSource(
    int ExternalId,
    string ProviderName,
    string Type,
    Uri Url,
    string Quality,
    double? Price
);