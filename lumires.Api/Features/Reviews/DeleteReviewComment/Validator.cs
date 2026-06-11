using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Reviews.DeleteReviewComment;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ReviewId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_ReviewId_Invalid"]);
    }
}