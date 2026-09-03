using EventParking.API.Configurations;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace EventParking.API.Services;

public sealed class DevelopmentEmailSender : IEmailSender
{
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(
        IOptions<FrontendOptions> frontendOptions,
        ILogger<DevelopmentEmailSender> logger)
    {
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        ApplicationUser user,
        string token)
    {
        var frontendBaseUrl = GetFrontendBaseUrl();

        var verificationUrl =
            $"{frontendBaseUrl}/verify-email" +
            $"?userId={Uri.EscapeDataString(user.Id)}" +
            $"&token={Uri.EscapeDataString(token)}";

        _logger.LogInformation(
            """
            SIMULATED EMAIL
            Type: Email Verification
            To: {Email}
            Verification URL: {VerificationUrl}
            """,
            user.Email,
            verificationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        ApplicationUser user,
        string token)
    {
        var frontendBaseUrl = GetFrontendBaseUrl();

        var resetUrl =
            $"{frontendBaseUrl}/reset-password" +
            $"?userId={Uri.EscapeDataString(user.Id)}" +
            $"&token={Uri.EscapeDataString(token)}";

        _logger.LogInformation(
            """
            SIMULATED EMAIL
            Type: Password Reset
            To: {Email}
            Reset URL: {ResetUrl}
            """,
            user.Email,
            resetUrl);

        return Task.CompletedTask;
    }

    private string GetFrontendBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(_frontendOptions.BaseUrl))
        {
            throw new InvalidOperationException(
                "Frontend:BaseUrl is not configured.");
        }

        return _frontendOptions.BaseUrl.TrimEnd('/');
    }
}
