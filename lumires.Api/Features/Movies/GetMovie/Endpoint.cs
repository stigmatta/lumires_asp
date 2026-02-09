using Ardalis.Result;
using Contracts.Abstractions;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed record Request(int Id);


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
    LocalizationResponse? Localization
);


internal class Endpoint(
    IExternalMovieService externalMovieService,
    ICurrentUserService currentUserService,
    IFusionCache cache,
    Queries queries)
    : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/movies/get/{id:int}");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = $"movie:{req.Id}:{lang}";

        ResultStatus? failureStatus = null;

        var movie = await cache.GetOrSetAsync<Response?>(
            cacheKey,
            async _ =>
            {
                var existingMovie = await queries.GetMovieByIdAsync(req.Id, lang, ct);
                if (existingMovie is not null)
                    return existingMovie;

                var result = await externalMovieService.GetMovieDetailsAsync(req.Id, lang, ct);

                if (!result.IsSuccess)
                {
                    failureStatus = result.Status;
                    return null;
                }

                var importedMovie = result.Value;
                var command = new ImportedEvent { TmdbId = importedMovie.ExternalId };
                await PublishAsync(command, Mode.WaitForNone, CancellationToken.None);
                
                var localizationResponse = new LocalizationResponse(lang, importedMovie.Title, importedMovie.Overview);
                return new Response(
                    importedMovie.ExternalId,
                    importedMovie.ReleaseDate.Year,
                    localizationResponse
                    );
            },
            options => options.SetDuration(CacheDuration.Medium), ct);

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