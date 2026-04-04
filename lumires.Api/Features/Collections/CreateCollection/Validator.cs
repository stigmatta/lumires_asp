using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Collections.CreateCollection;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(localizer["Collection_ValidationError_TitleEmpty"])
            .MinimumLength(StringLimits.MinLength)
            .WithMessage(localizer["Collection_ValidationError_TitleTooShort"])
            .MaximumLength(StringLimits.Default) 
            .WithMessage(localizer["Collection_ValidationError_TitleTooLong"]);

        RuleFor(x => x.Description)
            .MaximumLength(StringLimits.Description) 
            .WithMessage(localizer["Collection_ValidationError_DescriptionTooLong"])
            .When(x => x.Description is not null);

        RuleForEach(x => x.MovieIds)
            .NotEmpty()
            .WithMessage(localizer["Collection_ValidationError_InvalidMovieId"]);
            
    }
}