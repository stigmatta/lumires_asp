namespace Contracts.Models;

//TDOO this dto belongs only to the one endpoint. when GetMovieSources slice will be created - remove
public record MovieSource(
    int TmdbId,
    string ProviderName,
    string Type,
    Uri Url,
    string Quality,
    double? Price
);