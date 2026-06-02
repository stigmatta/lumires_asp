using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewsPreview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetReviewsPreviewAsync(CancellationToken ct)
    {
        var items = await db.Reviews
            .AsNoTracking()
            .OrderByDescending(r => r.LikesCount)
            .Select(r => new ReviewPreviewItem(r.Id,
                r.UserId,
                r.Reviewer.Username,
                r.Reviewer.AvatarUrl,
                r.Text,
                r.ReviewComments.Count,
                r.LikesCount,
                r.CreatedAt,
                r.ReviewComments
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
}