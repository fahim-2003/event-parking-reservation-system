using EventParking.API.Interfaces.Services;
using EventParking.API.Services;

namespace EventParking.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, DevelopmentEmailSender>();

        return services;
    }
}