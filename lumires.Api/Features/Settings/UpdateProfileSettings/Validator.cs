using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Settings.UpdateProfileSettings;

internal sealed class Validator : Validator<Command>
{
    public Validator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(StringLimits.Name)
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Username)
            .MaximumLength(StringLimits.Username)
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9._]*$")
            .When(x => x.Username is not null);

        RuleFor(x => x.Location)
            .MaximumLength(StringLimits.Default)
            .When(x => x.Location is not null);

        RuleFor(x => x.Tagline)
            .MaximumLength(StringLimits.Default)
            .When(x => x.Tagline is not null);

        RuleFor(x => x.Biography)
            .MaximumLength(StringLimits.Biography)
            .When(x => x.Biography is not null);
    }
}
