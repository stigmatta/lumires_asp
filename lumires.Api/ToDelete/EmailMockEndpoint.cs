using FastEndpoints;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.ToDelete;

//TODO remove
internal class EmailMockEndpoint(IEmailSenderService emailService)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/admin/test-email");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var model = new { Name = "Developer", Date = DateTime.Now };

        var message = new EmailSendCommand(
            "morrigun0@gmail.com",
            "Lumires Resend Test",
            "Welcome",
            model
        );

        await emailService.SendEmailAsync(message);
        await Send.OkAsync("Проверьте панель управления Resend или ваш почтовый ящик", ct);
    }
}