using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.FilmPeople.GetSimilarActors;

[UsedImplicitly]
internal sealed record Query(int Id);

[UsedImplicitly]
internal sealed record ActorItem(int ActorId, string? ProfilePath, string Name);

[UsedImplicitly]
internal sealed record Response(
    IReadOnlyCollection<ActorItem> SimilarActors);

internal sealed class Endpoint(
    IPersonResolver personResolver,
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/actors/{Id:int}/similar");
        Description(x => x.WithTags("People"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;

        var idAndDep = (query.Id, nameof(PersonDepartment.Acting));

        await personResolver.EnsurePersonExistsAsync(idAndDep, lang, ct);

        var response = await db.GetSimilarActors(query.Id, lang, ct);

        await Send.OkAsync(response, ct);
    }
}