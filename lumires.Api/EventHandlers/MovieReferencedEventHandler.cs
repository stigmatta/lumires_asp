using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Movies;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class MovieReferencedEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalMovieService externalMovieService,
    ILogger<MovieReferencedEventHandler> logger)
    : IEventHandler<MovieReferencedEvent>
{
    public async Task HandleAsync(MovieReferencedEvent command, CancellationToken ct)
    {
        var result = await externalMovieService
            .GetMovieDetailsAsync(command.ExternalId, command.Language, ct);

        if (!result.IsSuccess)
        {
            LogFailedImport(logger, command.ExternalId);
            return;
        }

        var data = result.Value;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var personResolver = scope.ServiceProvider.GetRequiredService<IPersonResolver>();

        var genreExternalIds = data.Genres.Items.Select(g => g.ExternalId).ToList();

        var genres = await db.Genres
            .Where(g => genreExternalIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var personDict = await personResolver.ResolveAsync(
            data.TopCast.Select(c => (c.Id, c.Name))
                .Concat(data.Directors.Select(d => (d.Id, d.Name))),
            ct);

        var movie = new Movie(
            Guid.CreateVersion7(),
            command.ExternalId,
            data.ReleaseDate,
            data.PosterPath,
            data.VoteAverage,
            data.VoteCount,
            data.Popularity,
            data.Runtime,
            data.ProductionCompany,
            data.BackdropPath,
            data.TrailerUrl
        );

        movie.AddGenres(genres);
        movie.AddSlug($"{data.Title}-{data.ReleaseDate.Year}");

        foreach (var c in data.TopCast.Where(c => personDict.ContainsKey(c.Id)))
            movie.AddCast(new MovieCast(personDict[c.Id].Id, c.Character, c.Order));

        foreach (var d in data.Directors.Where(d => personDict.ContainsKey(d.Id)))
            movie.AddDirector(new MovieDirector(personDict[d.Id].Id));

        movie.AddLocalization(new MovieLocalization(
            command.Language,
            data.Title,
            data.Overview));

        db.Movies.Add(movie);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            LogMovieAlreadyExists(logger, command.ExternalId);
        }
    }

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Failed to import movie {ExternalId}")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Movie already exists: {ExternalId}")]
    static partial void LogMovieAlreadyExists(ILogger logger, int externalId);
}