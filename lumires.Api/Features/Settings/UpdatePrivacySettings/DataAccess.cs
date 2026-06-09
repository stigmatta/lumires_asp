using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Settings.UpdatePrivacySettings;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UpdatePrivacySettings(Command command, Guid userId, CancellationToken ct)
    {
        var currentUser = await db.Users
            .Include(u => u.UserSettings)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (currentUser is null) return Result.Forbidden();

        currentUser.UserSettings.UpdatePrivacySettings(command.ProfileVisibility, command.IsAnyoneCanFollow,
            command.IsWatchlistPublic, command.AreLikesPublic,
            command.AreRatingsShowInFeeds);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}