namespace EventParking.API.Configurations;

public sealed class BookingSettings
{
    public const string SectionName = "BookingSettings";

    public int HoldMinutes { get; init; } = 15;
    public int ExpiryScanSeconds { get; init; } = 60;
}