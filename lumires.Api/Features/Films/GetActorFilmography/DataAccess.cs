using System.Collections.ObjectModel;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetActorFilmography;

[UsedImplicitly]
internal sealed record RawItem(
    int Id,
    string? PosterPath,
    string Title,
    string Slug,
    GenreItem[] Genres,
    int? ReleaseYear,
    float VoteAverage,
    int VoteCount);

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<IReadOnlyCollection<RawItem>> GetExistingFilms(int[] ids, string lang, CancellationToken ct)
    {
        var rawItems = await db.Films
            .AsNoTracking()
            .Where(x => ids.Contains(x.ExternalId))
            .Select(movie => new RawItem(
                movie.ExternalId,
                movie.PosterPath,
                movie.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                movie.Slug,
                movie.Genres
                    .Select(g => new GenreItem(
                        g.ExternalId,
                        g.Localizations
                            .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                            .OrderByDescending(gl => gl.LanguageCode == lang)
                            .Select(gl => gl.Name)
                            .FirstOrDefault() ?? string.Empty))
                    .ToArray(),
                movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.Year : null,
                movie.VoteAverage,
                movie.VoteCount
            ))
            .ToListAsync(ct);

        return new ReadOnlyCollection<RawItem>(rawItems);
    }

    internal async Task<Dictionary<int, GenreItem>> GetGenresDictionaryAsync(string lang, CancellationToken ct)
    {
        return await db.Genres
            .AsNoTracking()
            .Select(g => new GenreItem(
                g.ExternalId,
                g.Localizations
                    .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                    .OrderByDescending(gl => gl.LanguageCode == lang)
                    .Select(gl => gl.Name)
                    .FirstOrDefault() ?? string.Empty))
            .ToDictionaryAsync(g => g.Id, ct);
    }
}