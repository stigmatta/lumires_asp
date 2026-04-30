using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Auth.CreateProfile;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer["CreateProfile_ValidationError_Username_Empty"])
            .Must(User.IsUsernameValid)
            .WithMessage(localizer["CreateProfile_ValidationError_Username_Invalid"]);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["CreateProfile_ValidationError_Username_Empty"])
            .Must(User.IsEmailValid)
            .WithMessage(localizer["CreateProfile_ValidationError_Email_Invalid"]);
    }
}