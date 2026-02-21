using Core.Abstractions.Services;
using Core.Constants;
using Core.Events.Movies;
using FastEndpoints;
using JetBrains.Annotations;
using ZiggyCreatures.Caching.Fusion;

namespace Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record LocalizationResponse(
    string LanguageCode,
    string Title,
    string? Overview
);

[UsedImplicitly]
internal sealed record Response(
    int Id,
    int Year,
    string? TrailerUrl,
    string PosterPath,
    string? BackdropPath,
    LocalizationResponse? Localization
);

internal sealed class Endpoint(
    IExternalMovieService externalMovieService,
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DbQueries dbQueries)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/movies/get/{Id:int}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.MovieKey(query.Id, currentUserService.LangCulture);

        var movie = await cache.GetOrDefaultAsync<Response>(cacheKey, token: ct);
        if (movie is not null)
        {
            Response = movie;
            return;
        }

        var existingMovie = await dbQueries.GetMovieByIdAsync(query.Id, lang, ct);
        if (existingMovie is not null)
        {
            Response = existingMovie;
            return;
        }

        var externalMovie = await externalMovieService.GetMovieDetailsAsync(query.Id, lang, ct);

        if (!externalMovie.IsSuccess)
        {
            await HttpContext.SendErrorAsync(externalMovie, ct);
            return;
        }

        var importedMovie = externalMovie.Value;
        var command = new MovieReferencedEvent { ExternalId = importedMovie.ExternalId };
        await PublishAsync(command, Mode.WaitForNone, CancellationToken.None);

        LocalizationResponse localizationResponse = new(lang, importedMovie.Title, importedMovie.Overview);
        Response = new Response(
            importedMovie.ExternalId,
            importedMovie.ReleaseDate.Year,
            importedMovie.TrailerUrl,
            importedMovie.PosterPath,
            importedMovie.BackdropPath,
            localizationResponse
        );

        await cache.SetAsync(
            cacheKey,
            Response,
            options => options
                .SetDuration(CacheDuration.Medium)
                .SetFailSafe(true),
            ct
        );
    }
}