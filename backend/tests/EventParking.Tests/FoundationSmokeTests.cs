using EventParking.API.Configurations;
using EventParking.API.Data;
using Xunit;

namespace EventParking.Tests;

public class FoundationSmokeTests
{
    [Fact]
    public void ApiFoundation_ShouldLoadExpectedConfigurationDefaults()
    {
        var bookingSettings = new BookingSettings();

        Assert.Equal("BookingSettings", BookingSettings.SectionName);
        Assert.Equal(15, bookingSettings.HoldMinutes);
        Assert.Equal(60, bookingSettings.ExpiryScanSeconds);

        Assert.Equal("Jwt", JwtOptions.SectionName);
        Assert.Equal("Frontend", FrontendOptions.SectionName);

        var apiAssemblyName = typeof(AppDbContext).Assembly.GetName().Name;

        Assert.Equal("EventParking.API", apiAssemblyName);
    }
}