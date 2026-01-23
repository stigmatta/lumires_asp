using System.Globalization;
using FluentEmail.Core;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;
using lumires.Api.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Infrastructure.Services;

public sealed class EmailSenderService(IFluentEmail fluentEmail, IStringLocalizer<SharedResource> localizer) : IEmailSenderService
{
    public async Task SendEmailAsync(EmailSendCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var templatePath = Path.Combine(AppContext.BaseDirectory, $"{command.TemplateName}.cshtml");

        var email = await fluentEmail
            .To(command.To)
            .Subject(command.Subject)
            .UsingTemplateFromFile(templatePath, command.Model)
            .SendAsync();

        if (!email.Successful)
        {
            throw new InvalidOperationException(localizer["Error_EmailSendFailed"]);
        }
    }
}