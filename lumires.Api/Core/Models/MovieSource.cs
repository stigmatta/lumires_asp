namespace lumires.Api.Core.Models;

public record MovieSource(
    string ProviderName,
    string Type,
    Uri Url,
    string Quality,
    double? Price
);