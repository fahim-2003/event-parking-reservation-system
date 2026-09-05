using EventParking.API.Data;
using EventParking.API.DTOs.Dashboard;
using EventParking.API.Interfaces.Services.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardResponse> GetAdminDashboardAsync(
        CancellationToken cancellationToken)
    {
        var totalEvents =
            await _dbContext.Events.CountAsync(
                cancellationToken);

        var totalBookings =
            await _dbContext.Bookings.CountAsync(
                cancellationToken);

        var availableSeats =
            await _dbContext.Seats.CountAsync(
                seat => seat.Status == "Available",
                cancellationToken);

        var occupiedParkingSlots =
            await _dbContext.ParkingSlots.CountAsync(
                slot => slot.Status == "Booked",
                cancellationToken);

        var totalRevenue =
            await _dbContext.Payments
                .Where(payment => payment.Status == "Completed")
                .SumAsync(
                    payment => payment.Amount,
                    cancellationToken);

        var totalCustomers =
            await _dbContext.Users.CountAsync(
                cancellationToken);

        return new AdminDashboardResponse
        {
            TotalEvents = totalEvents,
            TotalBookings = totalBookings,
            AvailableSeats = availableSeats,
            OccupiedParkingSlots = occupiedParkingSlots,
            TotalRevenue = totalRevenue,
            TotalCustomers = totalCustomers
        };
    }


    public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        var upcomingBookings =
            await _dbContext.Bookings.CountAsync(
                booking =>
                    booking.CustomerId == customerId &&
                    booking.Status != "Cancelled",
                cancellationToken);


        var reservedParking =
            await _dbContext.Bookings.CountAsync(
                booking =>
                    booking.CustomerId == customerId &&
                    booking.ParkingSlotId != null,
                cancellationToken);


        var recentPayments =
            await _dbContext.Payments
                .Where(payment =>
                    _dbContext.Bookings.Any(
                        booking =>
                            booking.Id == payment.BookingId &&
                            booking.CustomerId == customerId))
                .CountAsync(
                    cancellationToken);


        var unreadNotifications = 0;


        return new CustomerDashboardResponse
        {
            UpcomingBookings = upcomingBookings,
            ReservedParking = reservedParking,
            RecentPayments = recentPayments,
            UnreadNotifications = unreadNotifications
        };
    }
}

