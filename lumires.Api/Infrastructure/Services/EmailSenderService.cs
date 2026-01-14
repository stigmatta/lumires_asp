using FluentEmail.Core;
using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Models;

namespace lumires.Api.Infrastructure.Services;

public class EmailSenderService(IFluentEmail fluentEmail) : IEmailSenderService
{
    public async Task SendEmailAsync(EmailSendCommand command)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), $"{command.TemplateName}.cshtml");

        var email = await fluentEmail
            .To(command.To)
            .Subject(command.Subject)
            .UsingTemplateFromFile(templatePath, command.Model)
            .SendAsync();

        if (!email.Successful) throw new Exception($"Failed to send email: {string.Join(", ", email.ErrorMessages)}");
    }
}