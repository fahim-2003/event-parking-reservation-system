using EventParking.API.Configurations;
using EventParking.API.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace EventParking.API.Services;

public sealed class BookingExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingSettings _settings;
    private readonly ILogger<BookingExpiryService> _logger;

    public BookingExpiryService(
        IServiceScopeFactory scopeFactory,
        IOptions<BookingSettings> settings,
        ILogger<BookingExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var delaySeconds =
            Math.Max(10, _settings.ExpiryScanSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var bookingService =
                    scope.ServiceProvider
                        .GetRequiredService<IBookingService>();

                var expiredCount =
                    await bookingService.ExpireHeldBookingsAsync(
                        stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation(
                        "Expired {ExpiredBookingCount} held bookings.",
                        expiredCount);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Booking expiry scan failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(delaySeconds),
                stoppingToken);
        }
    }
}
