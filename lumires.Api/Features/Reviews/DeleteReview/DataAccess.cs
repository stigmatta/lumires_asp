using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.DeleteReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteReviewAsync(Command command, Guid userId, string userRole, CancellationToken ct)
    {
        var review =
            await db.Reviews.FirstOrDefaultAsync(r => r.Id == command.ReviewId, ct);

        if (review is null) return Result.NoContent();
        if (review.UserId != userId || userRole == UserRoles.Admin || userRole == UserRoles.Moderator)
            return Result.Forbidden();

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}