using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Threads.DeleteThread;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ThreadId)
            .Must(x => x != Guid.Empty)
            .WithMessage(localizer["ValidationError_ThreadId_Invalid"]);
    }
}