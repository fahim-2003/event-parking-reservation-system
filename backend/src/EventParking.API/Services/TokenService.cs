using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventParking.API.Configurations;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventParking.API.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<ApplicationUser> userManager)
    {
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
    }

    public async Task<AccessTokenResult> CreateAccessTokenAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Email,
                user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _jwtOptions.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new AccessTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }
}