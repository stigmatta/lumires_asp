using Core.Abstractions.Data;
using Core.Abstractions.Services;
using Core.Constants;
using Core.Events.Movies;
using Domain.Entities;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Features.Movies.EventHandlers;

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

        await Task.WhenAll(fetchTasks.Values);

        var successfulResults = fetchTasks
            .Where(kvp => kvp.Value.Result.IsSuccess)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Result.Value
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

        var movie = new Movie
        {
            ExternalId = command.ExternalId,
            Year = defaultData.ReleaseDate.Year
        };

        foreach (var (culture, data) in successfulResults)
        {
            if (culture != defaultLang &&
                string.Equals(data.Overview, defaultData.Overview, StringComparison.Ordinal))
            {
                LogSkippingDuplicateLocalization(logger, culture, command.ExternalId);
                continue;
            }

            movie.Localizations.Add(new MovieLocalization
            {
                LanguageCode = culture,
                Title = data.Title,
                Description = data.Overview
            });
        }

        await using var scope = scopeFactory.CreateAsyncScope();

        await using var db = scope.ServiceProvider
            .GetRequiredService<IAppDbContext>();

        try
        {
            db.Movies.Add(movie);
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            LogMovieAlreadyExists(logger, command.ExternalId);
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, ex, command.ExternalId);

            throw;
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "No supported cultures configured.")]
    private static partial void LogNoSupportedCultures(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Failed to import movie {ExternalId}: no successful TMDB responses.")]
    private static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Movie already exists: {ExternalId}")]
    private static partial void LogMovieAlreadyExists(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Skipping {Culture} for movie {ExternalId} because it matches English fallback")]
    private static partial void LogSkippingDuplicateLocalization(
        ILogger logger,
        string culture,
        int externalId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Unexpected error while importing movie {ExternalId}")]
    private static partial void LogUnexpectedError(ILogger logger, Exception ex, int externalId);
}