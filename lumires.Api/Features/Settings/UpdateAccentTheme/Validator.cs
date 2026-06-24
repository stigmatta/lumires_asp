using FastEndpoints;
using FluentValidation;

namespace lumires.Api.Features.Settings.UpdateAccentTheme;

internal sealed class Validator : Validator<Command>
{
    private static readonly string[] AllowedAccentThemes =
    [
        "golden-hour", "film-noir", "crimson", "rose-pavilion",
        "twilight", "midnight", "blue-velvet", "celluloid"
    ];

    public Validator()
    {
        RuleFor(x => x.AccentTheme)
            .Must(theme => AllowedAccentThemes.Contains(theme))
            .When(x => x.AccentTheme is not null)
            .WithMessage("AccentTheme must be one of the allowed theme ids.");
    }
}
