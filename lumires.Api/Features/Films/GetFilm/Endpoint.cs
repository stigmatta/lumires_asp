using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Domain.Exceptions;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Films.GetFilm;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record LocalizationResponse(
    string LanguageCode,
    string Title,
    string? Overview,
    string? Tagline
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
    IFilmResolver filmResolver,
    IFusionCache cache,
    DataAccess dataAccess)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Slug}/{Id:int}");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.FilmKey(query.Id, lang);

        try
        {
            Response = await cache.GetOrSetAsync<Response>(
                cacheKey,
                async (_, token) =>
                {
                    await filmResolver.EnsureFilmExistsAsync(query.Id, lang, token);

                    return await dataAccess.GetFilmByIdAsync(query.Id, lang, token)
                           ?? throw new ExternalFilmException(ResultStatus.NotFound, "Film not found");
                },
                options => options.SetDuration(CacheDuration.Medium)
                    .SetFailSafe(true),
                ct
            );
        }
        catch (ExternalFilmException ex)
        {
            await HttpContext.SendErrorAsync(ex.Status, ct);
        }
    }
}