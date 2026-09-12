using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;

namespace EventParking.API.Services;

public sealed class DevelopmentSmsSender : ISmsSender
{
    private readonly ILogger<DevelopmentSmsSender> _logger;

    public DevelopmentSmsSender(
        ILogger<DevelopmentSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetOtpAsync(
        ApplicationUser user,
        string otp)
    {
        _logger.LogInformation(
            """
            SIMULATED SMS
            Type: Password Reset OTP
            To: {PhoneNumber}
            OTP: {Otp}
            """,
            user.PhoneNumber,
            otp);

        return Task.CompletedTask;
    }
}
