using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.Movies;
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
internal sealed record Response(
    Guid Id,
    DateOnly ReleaseDate,
    string? TrailerUrl,
    string PosterPath,
    string? BackdropPath,
    LocalizationResponse? Localization
);

internal sealed class Endpoint(
    IExternalMovieService externalMovieService,
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/movies/{Id:int}");
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
                    var existingMovie = await dataAccess.GetMovieByIdAsync(query.Id, lang, token);
                    if (existingMovie is not null)
                        return existingMovie;

                    var externalResult = await externalMovieService.GetMovieDetailsAsync(query.Id, lang, token);
                    if (!externalResult.IsSuccess)
                        throw new ExternalMovieException(externalResult.Status, "External service failed while retrieving the movie"); 

                    var importedMovie = externalResult.Value;
                    var internalId = Guid.CreateVersion7();

                    await PublishAsync(new MovieReferencedEvent
                    {
                        InternalId = internalId,
                        ExternalId = importedMovie.ExternalId,
                    }, Mode.WaitForAll, token);

                    return new Response(
                        internalId,
                        importedMovie.ReleaseDate,
                        importedMovie.TrailerUrl,
                        importedMovie.PosterPath,
                        importedMovie.BackdropPath,
                        new LocalizationResponse(lang, importedMovie.Title, importedMovie.Overview)
                    );
                },
                options => options.SetDuration(CacheDuration.Medium).SetFailSafe(true),
                ct
            );
        }
        catch (ExternalMovieException ex)
        {
            await HttpContext.SendErrorAsync(ex.Status, ct);
        }
    }
}