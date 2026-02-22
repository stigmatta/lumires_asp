using FluentEmail.Core.Interfaces;
using Infrastructure.Options;
using Infrastructure.Services;
using lumires.Core.Abstractions.Services;
using Resend;
using Resend.FluentEmail;

namespace Infrastructure.Extensions;

internal static class EmailSenderExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment env)
    {
        var emailConfig = configuration
                              .GetSection(EmailSenderConfig.Section)
                              .Get<EmailSenderConfig>() ??
                          throw new InvalidOperationException("Email sender configuration is missing.");

        var builder = services
            .AddFluentEmail(emailConfig.FromEmail, emailConfig.FromName)
            .AddRazorRenderer();

        if (env.IsDevelopment())
        {
            builder.AddSmtpSender("localhost", 25);
        }
        else
        {
            services.AddOptions<ResendClientOptions>()
                .Configure(o => o.ApiToken = configuration["Resend:ApiKey"]!);

            services.AddHttpClient<ResendClient>();
            services.AddTransient<IResend, ResendClient>();

            services.AddTransient<ISender, ResendSender>();
        }

        services.AddScoped<IEmailSenderService, EmailSenderService>();
        return services;
    }
}