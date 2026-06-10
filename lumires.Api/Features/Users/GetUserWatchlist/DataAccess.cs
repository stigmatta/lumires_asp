using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Core.Helpers;
using lumires.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserWatchlist;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Result<PagedResponse<CommonFilmListResponse>>> GetWatchlistAsync(
        Query query, string lang, Guid currentUserId, CancellationToken ct)
    {
        var target = await db.Users
            .Where(u => u.Username == query.Username)
            .Select(u => new { u.Id, u.UserSettings.IsWatchlistPublic })
            .FirstOrDefaultAsync(ct);

        if (target is null) return Result.NotFound();

        if (!target.IsWatchlistPublic && target.Id != currentUserId)
            return Result.Forbidden();

        var filter = Specifications.BuildFilter(query, lang);
        var sort = Specifications.BuildSort(query);

        var queryable = db.Films
            .Where(f => db.WatchlistFilms.Any(w => w.FilmId == f.Id && w.UserId == target.Id))
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        var rawItems = await queryable
            .Select(f => new
            {
                Film = f,
                Title = f.Localizations
                    .Where(l => l.Film.ExternalId == f.ExternalId &&
                                (l.LanguageCode == lang || l.LanguageCode == LocalizationConstants.DefaultCulture))
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                Genres = f.Genres
                    .Select(g => g.Localizations
                        .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == LocalizationConstants.DefaultCulture)
                        .OrderByDescending(gl => gl.LanguageCode == lang)
                        .Select(gl => gl.Name)
                        .FirstOrDefault() ?? string.Empty)
                    .ToArray()
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x =>
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
        }).ToList();

        var count = await db.Films
            .Where(f => db.WatchlistFilms.Any(w => w.FilmId == f.Id && w.UserId == target.Id))
            .ApplyFilter(filter)
            .CountAsync(ct);

        return Result.Success(new PagedResponse<CommonFilmListResponse>(
            items, count, query.Page, query.PageSize));
    }
}
