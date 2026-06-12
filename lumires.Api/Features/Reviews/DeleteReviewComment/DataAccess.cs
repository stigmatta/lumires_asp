using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.DeleteReviewComment;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteReviewCommentAsync(Command command, Guid userId, string userRole, CancellationToken ct)
    {
        var reviewComment =
            await db.ReviewComments.FirstOrDefaultAsync(r => r.Id == command.ReplyId, ct);

        if (reviewComment is null) return Result.NoContent();
        
        if (reviewComment.UserId != userId || userRole == UserRoles.Admin || userRole == UserRoles.Moderator)
            return Result.Forbidden();

        db.ReviewComments.Remove(reviewComment);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}