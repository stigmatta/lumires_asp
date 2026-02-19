using Ardalis.Result;
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
        Get("/movies/get/{id:int}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query req, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.MovieKey(req.Id, currentUserService.LangCulture);

        ResultStatus? failureStatus = null;

        var movie = await cache.GetOrSetAsync<Response?>(
            cacheKey,
            async _ =>
            {
                var existingMovie = await dbQueries.GetMovieByIdAsync(req.Id, lang, ct);
                if (existingMovie is not null)
                    return existingMovie;

                var externalMovie = await externalMovieService.GetMovieDetailsAsync(req.Id, lang, ct);

                if (!externalMovie.IsSuccess)
                {
                    failureStatus = externalMovie.Status;
                    return null;
                }

                var importedMovie = externalMovie.Value;
                var command = new MovieReferencedEvent { ExternalId = importedMovie.ExternalId };
                await PublishAsync(command, Mode.WaitForNone, CancellationToken.None);

                LocalizationResponse localizationResponse = new(lang, importedMovie.Title, importedMovie.Overview);
                return new Response(
                    Id: importedMovie.ExternalId,
                    Year: importedMovie.ReleaseDate.Year,
                    TrailerUrl: importedMovie.TrailerUrl,
                    PosterPath: importedMovie.PosterPath,
                    BackdropPath: importedMovie.BackdropPath,
                    Localization: localizationResponse
                );
            },
            options =>
                options.SetDuration(CacheDuration.Medium)
                    .IsFailSafeEnabled = true, ct);

        if (movie is null)
        {
            await cache.RemoveAsync(cacheKey, token: ct);

            switch (failureStatus)
            {
                case ResultStatus.Unauthorized:
                    await Send.UnauthorizedAsync(ct);
                    return;
                case ResultStatus.NotFound:
                    await Send.NotFoundAsync(ct);
                    return;
                default:
                    await Send.ErrorsAsync(500, ct);
                    return;
            }
        }

        Response = movie;
    }
}