using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Reviews.CreateReview;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FilmId)
            .Must(x => x != 0)
            .WithMessage(localizer["ValidationError_FilmId_Invalid"]);
        RuleFor(x => x.Title)
            .MinimumLength(StringLimits.MinLength)
            .WithMessage(localizer["ValidationError_Title_TooShort"])
            .MaximumLength(StringLimits.Default)
            .WithMessage(localizer["ValidationError_Title_TooLong"])
            .When(x => x.Title is not null);

        RuleFor(x => x.Text)
            .MaximumLength(StringLimits.Description)
            .WithMessage(localizer["ValidationError_Description_TooLong"]);

        RuleFor(x => x.Rating)
            .Must(x => x is >= 0f and <= 5f)
            .Must(x => x % 0.5f == 0)
            .WithMessage(localizer["ValidationError_Rating_Invalid"])
            .When(x => x.Rating is not null);
    }
}