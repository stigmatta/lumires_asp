using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Settings.UpdateAccentTheme;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UpdateAccentTheme(Command command, Guid userId, CancellationToken ct)
    {
        var currentUser = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (currentUser is null) return Result.Forbidden();

        currentUser.SetAccentTheme(command.AccentTheme);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
