using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Reviews.CreateReview;

[UsedImplicitly]
internal sealed record Command(int FilmId, string? Title, string Text, float? Rating, bool IsSpoilerFree);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string? Title,
    string Text,
    float? Rating,
    DateOnly CreatedAt
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/films/{Slug}/{filmId:int}/reviews/");
        Description(x => x.WithTags("Reviews"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.CreateReviewAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        var response = new Response(
            result.Value,
            command.Title,
            command.Text,
            command.Rating,
            DateOnly.FromDateTime(DateTime.UtcNow)
        );
        await Send.CreatedAtAsync<GetReview.Endpoint>(
            new { id = response.Id },
            response,
            cancellation: ct);
    }
}