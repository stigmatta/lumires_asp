using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.FilmsLists.CreateFilmsList;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(localizer["ValidationError_Title_Empty"])
            .MinimumLength(StringLimits.MinLength)
            .WithMessage(localizer["ValidationError_Title_TooShort"])
            .MaximumLength(StringLimits.Default)
            .WithMessage(localizer["ValidationError_Title_TooLong"]);

        RuleFor(x => x.Description)
            .MaximumLength(StringLimits.Description)
            .WithMessage(localizer["ValidationError_Description_TooLong"])
            .When(x => x.Description is not null);

        RuleForEach(x => x.FilmIds)
            .NotEmpty()
            .Must(x => x != 0)
            .WithMessage(localizer["ValidationError_FilmId_Invalid"]);
    }
}