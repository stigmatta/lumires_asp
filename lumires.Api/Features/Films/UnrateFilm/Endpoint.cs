using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.UnrateFilm;

[UsedImplicitly]
internal sealed record Command(int FilmId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/films/{Slug}/{filmId:int}/unrate/");
        Description(x => x.WithTags("Films"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.UnrateFilmAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}