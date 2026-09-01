using EventParking.API.Identity;

namespace EventParking.API.Interfaces.Services;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user);
}