using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Films.RateFilm;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FilmId)
            .Must(x => x != 0)
            .WithMessage(localizer["ValidationError_FilmId_Invalid"]);

        RuleFor(x => x.Rating)
            .Must(x => x is >= 0f and <= 5f)
            .Must(x => x % 0.5f == 0)
            .WithMessage(localizer["ValidationError_Rating_Invalid"]);
    }
}