using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Threads.UpdateThreadComment;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ThreadId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_ThreadId_Invalid"]);

        RuleFor(x => x.Text)
            .MinimumLength(StringLimits.MinLength)
            .WithMessage(localizer["ValidationError_Title_TooShort"])
            .MaximumLength(StringLimits.Default)
            .WithMessage(localizer["ValidationError_Title_TooLong"])
            .NotNull()
            .WithMessage(localizer["ValidationError_Text_CannotBeNull"]);

        RuleFor(x => x.TargetedUserId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_UserId_Invalid"])
            .When(x => x.TargetedUserId is not null);
    }
}