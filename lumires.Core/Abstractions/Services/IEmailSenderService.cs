using Core.Models;

namespace Core.Abstractions.Services;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailSendCommand command);
}