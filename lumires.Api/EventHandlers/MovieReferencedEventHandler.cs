using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.Movies;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class MovieReferencedEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalMovieService externalMovieService,
    ILogger<MovieReferencedEventHandler> logger,
    IOptions<RequestLocalizationOptions> locOptions)
    : IEventHandler<MovieReferencedEvent>
{
    public async Task HandleAsync(MovieReferencedEvent command, CancellationToken ct)
    {
        var cultures = locOptions.Value.SupportedCultures;

        if (cultures is null || cultures.Count == 0)
        {
            LogNoSupportedCultures(logger);
            return;
        }

        var fetchTasks = cultures.ToDictionary(
            c => c.Name,
            c => externalMovieService.GetMovieDetailsAsync(command.ExternalId, c.Name, ct)
        );

        var results = await Task.WhenAll(fetchTasks.Values);

        var successfulResults = fetchTasks
            .Zip(results)
            .Where(x => x.Second.IsSuccess)
            .ToDictionary(
                x => x.First.Key,
                x => x.Second.Value
            );

        if (successfulResults.Count == 0)
        {
            LogFailedImport(logger, command.ExternalId);
            return;
        }

        const string defaultLang = LocalizationConstants.DefaultCulture;

        var defaultData =
            successfulResults.TryGetValue(defaultLang, out var def)
                ? def
                : successfulResults.Values.First();
        
        await using var scope = scopeFactory.CreateAsyncScope();

        await using var db = scope.ServiceProvider
            .GetRequiredService<IAppDbContext>();
        

        if (await db.Movies.AnyAsync(x => x.ExternalId == command.ExternalId, ct))
        {
            LogMovieAlreadyExists(logger, command.ExternalId);
            return;
        }

        var genreExternalIds = defaultData.Genres.Items.Select(g => g.ExternalId).ToList();
        var genres = await db.Genres
            .Where(g => genreExternalIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var movie = new Movie(
            command.ExternalId,
            defaultData.ReleaseDate,
            defaultData.PosterPath,
            defaultData.VoteAverage,
            defaultData.VoteCount,
            defaultData.Popularity,
            defaultData.BackdropPath,
            defaultData.TrailerUrl
        );
        movie.AddGenres(genres);


        foreach (var (culture, data) in successfulResults)
        {
            if (culture != defaultLang &&
                string.Equals(data.Overview, defaultData.Overview, StringComparison.Ordinal))
            {
                LogSkippingDuplicateLocalization(logger, culture, command.ExternalId);
                continue;
            }

            var newLocalization = new MovieLocalization(culture, data.Title, data.Overview);
            movie.AddLocalization(newLocalization);
        }
        

        if (await db.Movies.AnyAsync(x => x.ExternalId == command.ExternalId, ct))
        {
            LogMovieAlreadyExists(logger, command.ExternalId);
            return;
        }

        try
        {
            db.Movies.Add(movie);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, command.ExternalId, ex);
            throw;
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "No supported cultures configured.")]
    static partial void LogNoSupportedCultures(ILogger<MovieReferencedEventHandler> logger);


    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Failed to import movie {ExternalId}: no successful TMDB responses.")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Movie already exists: {ExternalId}")]
    static partial void LogMovieAlreadyExists(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Skipping {Culture} for movie {ExternalId} because it matches English fallback")]
    static partial void LogSkippingDuplicateLocalization(
        ILogger logger,
        string culture,
        int externalId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Unexpected error while importing movie {ExternalId}")]
    static partial void LogUnexpectedError(
        ILogger logger,
        int externalId,
        Exception exception);
}