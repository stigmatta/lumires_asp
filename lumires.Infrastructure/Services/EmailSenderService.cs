using Contracts.Abstractions;
using Contracts.Models;
using FluentEmail.Core;

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