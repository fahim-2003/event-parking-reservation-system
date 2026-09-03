using Microsoft.OpenApi.Models;

namespace EventParking.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerWithJwtAuthentication(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer {your JWT token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition(
                "Bearer",
                securityScheme);

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        securityScheme,
                        Array.Empty<string>()
                    }
                });
        });

        return services;
    }
}