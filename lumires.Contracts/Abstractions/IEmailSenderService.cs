using Contracts.Models;

namespace Contracts.Abstractions;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailSendCommand command);
}