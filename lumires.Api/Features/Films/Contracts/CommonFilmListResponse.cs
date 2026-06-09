using JetBrains.Annotations;

namespace lumires.Api.Features.Films.Contracts;

[UsedImplicitly]
internal record CommonFilmListResponse(int Id, string Title, string? PosterPath, int? ReleaseYear, string[] Genres, float VoteAverage);
