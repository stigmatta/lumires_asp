using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.AddFilmToList;

[UsedImplicitly]
internal sealed record Command(Guid ListId, int FilmId);

[UsedImplicitly]
internal sealed record Response(bool ContainsFilm);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess,
    IFilmResolver filmResolver)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/lists/{listId:guid}/films/{filmId:int}");
        Description(x => x.WithTags("Lists"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;

        await filmResolver.EnsureFilmExistsAsync(command.FilmId, lang, ct);

        var result = await dataAccess.AddFilmAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.OkAsync(new Response(true), ct);
    }
}
