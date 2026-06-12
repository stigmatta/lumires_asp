using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.FilmsLists.DeleteFilmsList;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ListId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_ListId_Invalid"]);
    }
}