using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailSendCommand command);
}