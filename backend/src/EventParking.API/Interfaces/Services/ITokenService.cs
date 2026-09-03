using EventParking.API.Identity;
using EventParking.API.Services;

namespace EventParking.API.Interfaces.Services;

public interface ITokenService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(
        ApplicationUser user);
}