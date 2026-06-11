using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.UpdateFilmsList;

[UsedImplicitly]
internal sealed record Command(
    Guid ListId,
    string Title,
    string? Description,
    bool IsPrivate,
    IReadOnlyCollection<int> FilmIds
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess, IFilmResolver filmResolver)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Put("/lists/{listId:guid}");
        Description(x => x.WithTags("Lists"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;
        
        await filmResolver.EnsureFilmsExistAsync(command.FilmIds, lang, ct);
        var result = await dataAccess.UpdateFilmsListAsync(command, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.Invalid)
            {
                foreach (var error in result.ValidationErrors)
                    AddError(error.ErrorMessage);

                await Send.ErrorsAsync(400, ct);
                return;
            }

            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}