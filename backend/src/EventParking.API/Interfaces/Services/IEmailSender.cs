using EventParking.API.Identity;

namespace EventParking.API.Interfaces.Services;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        ApplicationUser user,
        string token);

    Task SendPasswordResetAsync(
        ApplicationUser user,
        string token);
}