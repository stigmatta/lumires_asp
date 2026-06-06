using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetTrendingFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response> GetTrendingFilmsAsync(string lang, CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.Films
            .AsNoTracking()
            .OrderByDescending(x =>
                x.Reviews.Count(r => r.CreatedAt >= weekAgo) +
                x.Likes.Count(l => l.LikedAt >= weekAgo))
            .Take(6)
            .Select(x => new
            {
                Film = x,
                TopReview = x.Reviews
                    .OrderByDescending(r => r.LikesCount)
                    .FirstOrDefault()
            })
            .Select(x => new TrendingItem(
                x.Film.Id,
                x.Film.ExternalId,
                x.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                x.Film.Slug,
                !string.IsNullOrWhiteSpace(x.TopReview!.Title)
                    ? x.TopReview.Title
                    : null,
                x.TopReview != null
                    ? x.TopReview.Rating
                    : null,
                x.TopReview != null
                    ? x.TopReview.UserId
                    : Guid.Empty,
                x.TopReview != null
                    ? x.TopReview.Reviewer.Username
                    : string.Empty
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}