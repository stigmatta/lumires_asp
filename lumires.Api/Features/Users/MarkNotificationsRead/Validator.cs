using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Users.MarkNotificationsRead;

internal sealed class Validator : Validator<Command>
{
    public Validator()
    {
        RuleForEach(x => x.Ids)
            .Must(x => x != Guid.Empty);
    }
}