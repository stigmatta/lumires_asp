using JetBrains.Annotations;

namespace Infrastructure.Services.Watchmode;

[UsedImplicitly]
public sealed record WatchmodeSearchResponse(
    IReadOnlyList<WatchmodeTitleResult> TitleResults
);

[UsedImplicitly]
public sealed record WatchmodeTitleResult(
    int Id,
    string Name,
    int TmdbId,
    string TmdbType
);

[UsedImplicitly]
public sealed record WatchmodeSourceResponse(
    string Name,
    string Type,
    Uri WebUrl,
    string Format,
    double? Price
);