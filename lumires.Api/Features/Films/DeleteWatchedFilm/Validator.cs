using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Films.DeleteWatchedFilm;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FilmId)
            .Must(x => x != 0)
            .WithMessage(localizer["ValidationError_FilmId_Invalid"]);
    }
}