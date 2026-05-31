using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Threads.CreateThread;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Title)
            .MinimumLength(StringLimits.MinLength)
            .WithMessage(localizer["ValidationError_Title_TooShort"])
            .MaximumLength(StringLimits.Default)
            .WithMessage(localizer["ValidationError_Title_TooLong"])
            .When(x => x.Title is not null);

        RuleFor(x => x.Text)
            .MaximumLength(StringLimits.Description)
            .WithMessage(localizer["ValidationError_Description_TooLong"]);
    }
}