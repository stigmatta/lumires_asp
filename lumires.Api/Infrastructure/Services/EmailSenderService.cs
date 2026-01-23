using System.Globalization;
using FluentEmail.Core;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.Infrastructure.Services;

public sealed class EmailSenderService(IFluentEmail fluentEmail) : IEmailSenderService
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
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to send email to {0}: {1}", 
                    command.To, 
                    string.Join(", ", email.ErrorMessages)));
        }    }
}