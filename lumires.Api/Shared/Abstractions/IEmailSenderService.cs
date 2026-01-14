using lumires.Api.Shared.Models;

namespace lumires.Api.Shared.Abstractions;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailSendCommand command);
}