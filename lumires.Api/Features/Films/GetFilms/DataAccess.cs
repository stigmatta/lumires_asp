using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<List<FilmItemResponse>> GetFilmsAsync(Query query, string lang, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);
        var sort = Specifications.BuildSort(query);

        var queryable = db.Films
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(f => new FilmItemResponse(
                f.ExternalId,
                f.Localizations.Where(l =>
                        l.Film.ExternalId == f.ExternalId && (l.LanguageCode == lang ||
                                                              l.LanguageCode == LocalizationConstants.DefaultCulture))
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                f.ReleaseDate.HasValue ? f.ReleaseDate.Value.Year : null,
                f.Genres
                    .Select(g => g.Localizations
                        .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == LocalizationConstants.DefaultCulture)
                        .OrderByDescending(gl => gl.LanguageCode == lang)
                        .Select(gl => gl.Name)
                        .FirstOrDefault() ?? string.Empty)
                    .ToArray(),
                f.VoteAverage,
                f.PosterPath
            ))
            .ToListAsync(ct);
    }

    public async Task<int> GetFilmsCountAsync(Query query, string lang, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);
        return await db.Films
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}