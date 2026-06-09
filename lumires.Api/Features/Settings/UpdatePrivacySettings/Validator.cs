using FastEndpoints;
using FluentValidation;
using lumires.Core.Constants;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Settings.UpdatePrivacySettings;

internal sealed class Validator : Validator<Command>
{
    public Validator()
    {
    }
}
