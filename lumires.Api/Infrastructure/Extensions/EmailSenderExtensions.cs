using FluentEmail.Core.Interfaces;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Options;
using lumires.Api.Infrastructure.Services;
using Resend;
using Resend.FluentEmail;

namespace lumires.Api.Infrastructure.Extensions;

internal static class EmailSenderExtensions
{
    public static IServiceCollection AddLumiresEmail(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment env)
    {
        var emailConfig = configuration
            .GetSection(EmailSenderConfig.Section)
            .Get<EmailSenderConfig>() ?? new EmailSenderConfig();

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