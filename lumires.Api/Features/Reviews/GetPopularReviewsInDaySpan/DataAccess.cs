using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetPopularReviewsInDaySpan;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetPopularReviewsBySpan(int daySpan, string lang, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var startDate = DateTime.UtcNow.AddDays(-daySpan);

        var items = await db.Reviews
            .AsNoTracking()
            .Where(r => r.IsSpoilerFree && r.CreatedAt >= startDate)
            .OrderByDescending(r => r.LikesCount)
            .ThenByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new PopularReviewItem(
                r.Id,
                r.Film.ExternalId,
                r.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                r.Film.Slug,
                r.Film.PosterPath,
                r.Film.ReleaseDate.HasValue ? r.Film.ReleaseDate.Value.Year : null,
                r.Film.Genres
                    .Select(g => g.Localizations
                        .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == LocalizationConstants.DefaultCulture)
                        .OrderByDescending(gl => gl.LanguageCode == lang)
                        .Select(gl => gl.Name)
                        .FirstOrDefault() ?? string.Empty)
                    .ToArray(),
                r.Film.Runtime,
                r.Film.Directors.First().PersonId,
                r.Film.Directors.First().Person.GetName(lang),
                r.Title,
                r.Text,
                r.UserId,
                r.Reviewer.Username,
                r.CreatedAt,
                r.Rating,
                r.LikesCount,
                r.ReviewComments.Count,
                currentUserId != Guid.Empty && r.Likes.Any(l => l.UserId == currentUserId),
                r.IsEditorPick,
                r.Text.Length / 5 / 200 + 1
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}