namespace EventParking.API.Configurations;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FullName { get; init; } = "System Administrator";
}