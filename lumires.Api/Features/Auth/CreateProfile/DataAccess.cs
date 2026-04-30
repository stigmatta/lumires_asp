using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Auth.CreateProfile;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<bool> IsUserExistsAsync(Command command, CancellationToken ct)
    {
        return await db.Users
            .AnyAsync(x => x.Id == command.Id || x.Username == command.Username || x.Email == command.Email, ct);
    }

    internal async Task<Response> CreateUserAsync(Command command, CancellationToken ct)
    {
        var newUser = new User(
            command.Id,
            command.Username,
            command.Email
        );

        db.Users.Add(newUser);
        await db.SaveChangesAsync(ct);

        var response = new Response(command.Id, command.Email, command.Username, null);
        return response;
    }
}