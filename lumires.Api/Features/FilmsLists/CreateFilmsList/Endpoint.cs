using Ardalis.Result;
using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.FilmsLists.CreateFilmsList;

[UsedImplicitly]
internal sealed record Command(
    string Title,
    string? Description,
    bool IsPrivate,
    IReadOnlyCollection<int> FilmIds
);

[UsedImplicitly]
internal sealed record Response(Guid FilmsListId, string Title, DateTimeOffset CreatedAt);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess, IFilmResolver filmResolver)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/lists/");
        Description(x => x.WithTags("Lists"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;
        
        await filmResolver.EnsureFilmsExistAsync(command.FilmIds, lang, ct);
        var result = await dataAccess.CreateFilmsListAsync(command, currentUserId, ct);

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

        var response = new Response(
            result.Value,
            command.Title,
            DateTimeOffset.UtcNow
        );
        await Send.CreatedAtAsync<GetFilmsList.Endpoint>(
            new { id = result.Value },
            response,
            cancellation: ct);
    }
}