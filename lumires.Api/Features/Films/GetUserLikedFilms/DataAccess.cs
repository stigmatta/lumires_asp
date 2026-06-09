using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserLikedFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<List<CommonFilmListResponse>> GetLikedFilmsAsync(Query query, string lang, Guid userId, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);
        var sort = Specifications.BuildSort(query);

        var queryable = db.Films
            .Where(f => f.Likes.Any(l => l.UserId == db.Users
                .Where(u => u.Username == query.Username)
                .Select(u => u.Id)
                .FirstOrDefault()))
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

    public async Task<int> GetFilmsCountAsync(Query query, string lang, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query, lang);
        return await db.Films
            .Where(f => f.Likes.Any(l => l.UserId == db.Users
                .Where(u => u.Username == query.Username)
                .Select(u => u.Id)
                .FirstOrDefault()))
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}