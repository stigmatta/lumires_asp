using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetMostReviewedFilmByActor;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetMostReviewedFilmByActor(
        int actorId, string lang, Guid currentUserId, CancellationToken ct)
    {
        return await db.FilmCasts
            .AsNoTracking()
            .Where(x => x.Person.ExternalId == actorId &&
                        x.Person.PersonDepartment == PersonDepartment.Acting)
            .OrderByDescending(x => x.Film.Reviews.Count)
            .Select(d => new
            {
                FilmId = d.Film.ExternalId,
                FilmTitle = d.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault(),
                d.Film.Slug,
                d.Film.PosterPath,
                ReviewsCount = d.Film.Reviews.Count,
                TopReview = d.Film.Reviews
                    .OrderByDescending(r => r.ReviewComments.Count)
                    .Select(r => new
                    {
                        r.UserId,
                        r.Reviewer.Username,
                        r.Reviewer.AvatarUrl,
                        r.Title,
                        r.Text,
                        r.LikesCount,
                        IsLikedByMe = r.Likes.Any(l => l.UserId == currentUserId),
                        Comments = r.ReviewComments
                            .OrderByDescending(c => c.CreatedAt)
                            .Take(3)
                            .Select(c => new ReviewCommentItem(
                                c.Id,
                                c.UserId,
                                c.Commentator.Username,
                                c.Text,
                                c.CreatedAt,
                                c.LikesCount,
                                c.Likes.Any(l => l.UserId == currentUserId)
                            ))
                            .ToList()
                    })
                    .FirstOrDefault()
            })
            .Where(x => x.TopReview != null)
            .Select(x => new Response(
                x.FilmId,
                x.FilmTitle,
                x.Slug,
                x.PosterPath,
                x.ReviewsCount,
                x.TopReview!.UserId,
                x.TopReview.Username,
                x.TopReview.AvatarUrl,
                x.TopReview.Title,
                x.TopReview.Text,
                x.TopReview.LikesCount,
                x.TopReview.IsLikedByMe,
                x.TopReview.Comments
            ))
            .FirstOrDefaultAsync(ct);
    }
}