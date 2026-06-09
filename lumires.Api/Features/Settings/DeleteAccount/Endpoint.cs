using FastEndpoints;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Settings.DeleteAccount;

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : EndpointWithoutRequest<EmptyResponse>
{
    public override void Configure()
    {
        Put("/settings/delete-account");
        Description(x => x.WithTags("Settings"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.DeleteAccount(currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);   
    }

}