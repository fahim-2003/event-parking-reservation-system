using EventParking.API.Identity;

namespace EventParking.API.Interfaces.Services;

public interface ISmsSender
{
    Task SendPasswordResetOtpAsync(
        ApplicationUser user,
        string otp);
}
