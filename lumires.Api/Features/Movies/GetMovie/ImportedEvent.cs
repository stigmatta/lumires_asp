using System.Diagnostics;
using Contracts.Abstractions;
using Contracts.Constants;
using Contracts.Models;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Domain.Entities;
using lumires.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace lumires.Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal class ImportedEvent : IEvent
{
    public int TmdbId { get; init; }
}

[UsedImplicitly]
internal partial class MovieImportedEventHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<MovieImportedEventHandler> logger,
    IOptions<RequestLocalizationOptions> locOptions)
    : IEventHandler<ImportedEvent>
{
    public async Task HandleAsync(ImportedEvent command, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tmdbService = scope.ServiceProvider.GetRequiredService<IExternalMovieService>();

        var cultures = locOptions.Value.SupportedCultures ?? [];
        var tasksByLang = cultures.ToDictionary(
            c => c.Name,
            c => tmdbService.GetMovieDetailsAsync(command.TmdbId, c.Name, ct)
        );

        await Task.WhenAll(tasksByLang.Values);
        
        var resultsByLang = tasksByLang
            .Where(kvp => kvp.Value.Result.IsSuccess)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result.Value);

        const string defLang = LocalizationConstants.DefaultCulture;
        
        if (!resultsByLang.TryGetValue(defLang, out var defData))
        {
            defData = resultsByLang.Values.First();
        }
        
        var movie = new Movie
        {
            TmdbId = command.TmdbId,
            Year = defData.ReleaseDate.Year
        };

        foreach (var (cultureName, data) in resultsByLang)
        {
            
            if (cultureName != defLang && 
                defData != null && 
                string.Equals(data.Overview, defData.Overview, StringComparison.Ordinal))
            {
                LogSkippingDuplicateLocalization(logger, cultureName, command.TmdbId);
                continue;
            }

            movie.Localizations.Add(new MovieLocalization
            {
                LanguageCode = cultureName,
                Title = data.Title,
                Description = data.Overview
            });
        }

        try {
            db.Movies.Add(movie);
            await db.SaveChangesAsync(ct);
        } catch (DbUpdateException ex) when (IsDuplicateKeyException(ex)) {
            LogMovieAlreadyExists(logger, command.TmdbId);
        }
    }
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Movie already exists: {TmdbId}")]
    static partial void LogMovieAlreadyExists(ILogger logger, int tmdbId);
    
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Skipping {Culture} for movie {TmdbId} because it matches English fallback")]
    static partial void LogSkippingDuplicateLocalization(ILogger logger, string culture, int tmdbId);

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pgEx)
            // 23505 = unique_violation
            return pgEx.SqlState == "23505";
        return false;
    }
}