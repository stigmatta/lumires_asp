using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Mappers;
using lumires.Domain.Enums;

namespace lumires.Api.Features.FilmPeople.GetActor;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record Response(
    int ActorId,
    string Lang,
    string Name,
    string? Biography,
    DateOnly? Birthday,
    DateOnly? Deathday,
    GenderType Gender,
    string? PlaceOfBirth,
    string? ProfilePath);

internal sealed class Endpoint(
    IPersonResolver personResolver,
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/actors/{Id:int}");
        Description(x => x.WithTags("People"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var idAndDep = (query.Id, nameof(PersonDepartment.Acting));

        await personResolver.EnsurePersonExistsAsync(idAndDep, lang, ct);

        var response = await db.GetActorByIdAsync(query.Id, lang, ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}