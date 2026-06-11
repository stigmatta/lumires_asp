using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Reviews.DeleteReview;

[UsedImplicitly]
internal sealed record Command(Guid ReviewId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/reviews/{reviewId:guid}");
        Description(x => x.WithTags("Reviews"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var userRole = currentUserService.UserRole;

        var result = await dataAccess.DeleteReviewAsync(command, currentUserId, userRole, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}