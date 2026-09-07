namespace EventParking.API.DTOs.Dashboard;

public sealed class CustomerDashboardResponse
{
    public int UpcomingBookings { get; set; }

    public int ReservedParking { get; set; }

    public int RecentPayments { get; set; }

    public int UnreadNotifications { get; set; }
}
