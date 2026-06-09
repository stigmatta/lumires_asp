using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Settings.UpdateProfileSettings;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UpdateProfileSettings(Command command, Guid userId, CancellationToken ct)
    {
        var currentUser = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (currentUser is null) return Result.Forbidden();

        currentUser.UpdateProfileSettings(command.AvatarUrl, command.DisplayName, command.Username, command.Pronouns,
            command.Location, command.Tagline, command.Biography);
        
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}