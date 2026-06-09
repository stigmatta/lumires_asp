using JetBrains.Annotations;

namespace lumires.Api.Features.Genres.Contracts;

[UsedImplicitly]
internal record GenreItem(
    int Id,
    string Name);
