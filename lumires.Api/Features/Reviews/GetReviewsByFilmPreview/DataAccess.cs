using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewsByFilmPreview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetReviewsPreviewAsync(int movieId, CancellationToken ct)
    {
        var items = await db.Reviews
            .AsNoTracking()
            .Where(r => r.Film.ExternalId == movieId && r.IsSpoilerFree)
            .OrderByDescending(r => r.LikesCount)
            .Select(r => new ReviewPreviewItem(r.Id, r.UserId, r.Reviewer.Username, r.Reviewer.AvatarUrl, r.Text,
                r.ReviewComments.Count, r.LikesCount, r.ReviewComments
                    .Where(c => c.TargetedUserId == r.UserId || c.TargetedUserId == null)
                    .OrderByDescending(c => c.LikesCount)
                    .Select(c => new ReviewCommentPreview(
                        c.Id,
                        c.UserId,
                        c.Commentator.Username,
                        c.Commentator.AvatarUrl,
                        c.Text
                    ))
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return new Response(items);
    }

    internal async Task<bool> FilmExistsAsync(int externalId, CancellationToken ct)
    {
        return await db.Films.AnyAsync(m => m.ExternalId == externalId, ct);
    }
}