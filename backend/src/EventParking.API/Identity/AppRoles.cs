namespace EventParking.API.Identity;

public static class AppRoles
{
    public const string Customer = "Customer";
    public const string Administrator = "Administrator";

    public static readonly string[] All =
    [
        Customer,
        Administrator
    ];
}