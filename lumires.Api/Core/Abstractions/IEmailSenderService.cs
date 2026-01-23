using lumires.Api.Core.Models;

namespace lumires.Api.Core.Abstractions;

internal interface IEmailSenderService
{
    Task SendEmailAsync(EmailSendCommand command);
}