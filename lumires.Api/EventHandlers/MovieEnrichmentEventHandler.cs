using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Movies;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class MovieEnrichmentEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalMovieService externalMovieService,
    ILogger<MovieReferencedEventHandler> logger,
    IOptions<RequestLocalizationOptions> locOptions)
    : IEventHandler<MovieEnrichmentEvent>
{
    public async Task HandleAsync(MovieEnrichmentEvent command, CancellationToken ct)
    {
        try
        {
            var cultures = locOptions.Value.SupportedCultures!
                .Select(c => c.Name)
                .Where(c => c != command.SkipLanguage);

            foreach (var culture in cultures)
            {
                var res = await externalMovieService
                    .GetMovieDetailsAsync(command.ExternalId, culture, CancellationToken.None);

                if (!res.IsSuccess)
                    continue;

                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var movie = await db.Movies
                    .FirstOrDefaultAsync(m => m.ExternalId == command.ExternalId, ct);

                if (movie is null)
                    continue;

                movie.AddLocalization(new MovieLocalization(
                    culture,
                    res.Value.Title,
                    res.Value.Overview));

                await db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, command.ExternalId, ex);
        }
    }

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Unexpected error while importing movie {ExternalId}")]
    static partial void LogUnexpectedError(
        ILogger logger,
        int externalId,
        Exception exception);
}