using Core.Abstractions.Services;
using Core.Resources;
using FastEndpoints;
using Microsoft.Extensions.Localization;

namespace Api.ToDelete;

internal class L10NTestEndpoint(IStringLocalizer<SharedResource> localizer, ICurrentUserService currentUserService)
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