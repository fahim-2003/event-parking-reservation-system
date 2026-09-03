using System.Text;
using EventParking.API.Configurations;
using EventParking.API.Interfaces.Services;
using EventParking.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EventParking.API.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration was not found.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
            jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 characters.");
        }

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.Key)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        NameClaimType =
                            System.Security.Claims.ClaimTypes.Name,

                        RoleClaimType =
                            System.Security.Claims.ClaimTypes.Role
                    };
            });

        services.AddAuthorization();

        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}