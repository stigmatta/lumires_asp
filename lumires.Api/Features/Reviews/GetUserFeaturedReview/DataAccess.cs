using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetUserFeaturedReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<ProfileFeaturedReview?> GetFeaturedReview(string username, string lang, Guid currentUserId,
        CancellationToken ct)
    {
        return await db.Reviews
            .Where(r => r.Reviewer.Username == username )
            .OrderByDescending(r => r.LikesCount)
            .Select(r => new ProfileFeaturedReview(
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
                    .Select(g =>
                        g.Localizations
                            .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                            .OrderByDescending(gl => gl.LanguageCode == lang)
                            .Select(gl => gl.Name)
                            .FirstOrDefault() ?? string.Empty)
                    .ToArray(),
                r.Film.Runtime,
                r.Film.Directors.First().PersonId,
                r.Film.Directors.First().Person.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? "Unknown",
                r.Title,
                r.Text,
                r.UserId,
                r.Reviewer.Username,
                r.Reviewer.AvatarUrl,
                r.CreatedAt,
                r.Rating,
                r.LikesCount,
                r.ReviewComments.Count,
                currentUserId != Guid.Empty && r.Likes.Any(l => l.UserId == currentUserId),
                r.IsEditorPick,
                r.Text.Length / 5 / 200 + 1,
                r.UserId == currentUserId
            ))
            .FirstOrDefaultAsync(ct);
    }
}