using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.DeleteThreadComment;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteThreadCommentAsync(Command command, Guid userId, string userRole, CancellationToken ct)
    {
        var threadComment =
            await db.ThreadComments.FirstOrDefaultAsync(r => r.Id == command.ReplyId, ct);

        if (threadComment is null) return Result.NoContent();
        
        if (threadComment.UserId != userId || userRole == UserRoles.Admin || userRole == UserRoles.Moderator)
            return Result.Forbidden();

        db.ThreadComments.Remove(threadComment);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}