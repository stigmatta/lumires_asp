using FluentEmail.Core;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace Infrastructure.Services;

public sealed class EmailSenderService(IFluentEmail fluentEmail) : IEmailSenderService
{
    public async Task SendEmailAsync(EmailSendCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Resources",
            "EmailTemplates",
            $"{command.TemplateName}.cshtml");

        await fluentEmail
            .To(command.To)
            .Subject(command.Subject)
            .UsingTemplateFromFile(templatePath, command.Model)
            .SendAsync();
    }
}