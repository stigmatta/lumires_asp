using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetUserWatchlist;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<List<CommonFilmListResponse>> GetWatchlistByUser(Query query, string lang, Guid userId, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);
        var sort = Specifications.BuildSort(query);

        var targetUserId = await db.Users
            .Where(u => u.Username == query.Username)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (targetUserId == Guid.Empty) return [];

        var queryable = db.WatchlistFilms
            .Where(w => w.UserId == targetUserId && 
                        (targetUserId == userId || w.User.UserSettings.IsWatchlistPublic))
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);
        
        var rawItems = await queryable
            .Select(f => new
            {
                f.Film,
                Title = f.Film.Localizations
                    .Where(l => l.Film.ExternalId == f.Film.ExternalId && 
                                (l.LanguageCode == lang || l.LanguageCode == LocalizationConstants.DefaultCulture))
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                Genres = f.Film.Genres
                    .Select(g => g.Localizations
                        .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == LocalizationConstants.DefaultCulture)
                        .OrderByDescending(gl => gl.LanguageCode == lang)
                        .Select(gl => gl.Name)
                        .FirstOrDefault() ?? string.Empty)
                    .ToArray()
            })
            .ToListAsync(ct);
        
        return [.. rawItems.Select(x =>
        {
            var (rating, _) = CalculateFilmRating.Handle(
                x.Film.VoteAverage,
                x.Film.VoteCount,
                x.Film.VoteAverage,  
                x.Film.VoteCount
            );

            return new CommonFilmListResponse(
                x.Film.ExternalId,
                x.Title,
                x.Film.PosterPath,
                x.Film.ReleaseDate?.Year,
                x.Genres,
                rating
            );
        })];
    }

    public async Task<int> GetFilmsCountAsync(Query query, string lang, Guid userId, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);

        var targetUserId = await db.Users
            .Where(u => u.Username == query.Username)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (targetUserId == Guid.Empty) return 0;

        return await db.WatchlistFilms
            .Where(w => w.UserId == targetUserId &&
                        (targetUserId == userId || w.User.UserSettings.IsWatchlistPublic))
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}