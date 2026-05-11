using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Domain.Exceptions;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record LocalizationResponse(
    string LanguageCode,
    string Title,
    string? Overview
);

[UsedImplicitly]
internal sealed record GenreItemResponse(
    int Id,
    string Name,
    string LanguageCode
);

[UsedImplicitly]
internal sealed record GenresResponse(
    IReadOnlyCollection<GenreItemResponse> Items
);

[UsedImplicitly]
internal sealed record Response(
    int Id,
    DateOnly ReleaseDate,
    string? TrailerUrl,
    string? PosterPath,
    string? BackdropPath,
    LocalizationResponse? Localization,
    GenresResponse Genres,
    IReadOnlyCollection<string> Cast,
    IReadOnlyCollection<string> Directors,
    string ProductionCompany,
    int Runtime
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IMovieResolver movieResolver,
    IFusionCache cache,
    DataAccess dataAccess)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/movies/{Slug}/{Id:int}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.MovieKey(query.Id, lang);

        try
        {
            Response = await cache.GetOrSetAsync<Response>(
                cacheKey,
                async (_, token) =>
                {
                    await movieResolver.EnsureMovieExistsAsync(query.Id, lang, token);

                    return await dataAccess.GetMovieByIdAsync(query.Id, lang, token)
                           ?? throw new ExternalMovieException(ResultStatus.NotFound, "Movie not found");
                },
                options => options.SetDuration(CacheDuration.Medium)
                    .SetFailSafe(true),
                ct
            );
        }
        catch (ExternalMovieException ex)
        {
            await HttpContext.SendErrorAsync(ex.Status, ct);
        }
    }
}