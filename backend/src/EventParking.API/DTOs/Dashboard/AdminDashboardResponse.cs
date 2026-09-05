namespace EventParking.API.DTOs.Dashboard;

public sealed class AdminDashboardResponse
{
    public int TotalEvents { get; set; }

    public int TotalBookings { get; set; }

    public int AvailableSeats { get; set; }

    public int OccupiedParkingSlots { get; set; }

    public decimal TotalRevenue { get; set; }

    public int TotalCustomers { get; set; }
}
