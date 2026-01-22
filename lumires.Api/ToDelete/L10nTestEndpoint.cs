using FastEndpoints;
using lumires.Api.Resources;
using lumires.Api.Shared.Abstractions;
using Microsoft.Extensions.Localization;

namespace lumires.Api.ToDelete;

public class L10nTestEndpoint(IStringLocalizer<SharedResource> localizer, ICurrentUserService currentUserService)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/l10n");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var welcomeMessage = localizer["Hello"];
        var response = new
        {
            Message = welcomeMessage.Value,
            DetectedLanguage = currentUserService.CurrentLanguage,
            Timestamp = DateTime.UtcNow.ToShortTimeString()
        };

        await Send.OkAsync(response, ct);
    }
}