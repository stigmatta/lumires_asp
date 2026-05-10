using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.LikeReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid reviewId, CancellationToken ct)
    {
        var review = await db.Reviews
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review is null) return Result.NotFound();

        var userId = currentUserService.UserId;
        var isLiked = review.ToggleLike(userId);

        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, review.LikesCount);
        return Result.Success(response);
    }
}