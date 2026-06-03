using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Relationships.UnblockUser;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.TargetUserId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_UserId_Invalid"]);
    }
}