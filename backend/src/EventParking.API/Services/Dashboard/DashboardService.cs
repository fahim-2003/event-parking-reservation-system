using EventParking.API.Data;
using EventParking.API.DTOs.Dashboard;
using EventParking.API.Interfaces.Services.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private const string CustomerRoleName = "CUSTOMER";
    private const string HeldStatus = "Held";
    private const string ConfirmedStatus = "Confirmed";
    private const string OccupiedParkingStatus = "Occupied";

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
                slot =>
                    slot.Status == OccupiedParkingStatus,
                cancellationToken);

        var totalRevenue =
            await _dbContext.Payments
                .Where(payment =>
                    payment.Status == "Completed")
                .SumAsync(
                    payment => payment.Amount,
                    cancellationToken);

        var customerRoleId =
            await _dbContext.Roles
                .Where(role =>
                    role.NormalizedName ==
                    CustomerRoleName)
                .Select(role => role.Id)
                .SingleOrDefaultAsync(
                    cancellationToken);

        var totalCustomers =
            string.IsNullOrWhiteSpace(customerRoleId)
                ? 0
                : await _dbContext.UserRoles
                    .CountAsync(
                        userRole =>
                            userRole.RoleId ==
                            customerRoleId,
                        cancellationToken);

        return new AdminDashboardResponse
        {
            TotalEvents = totalEvents,
            TotalBookings = totalBookings,
            AvailableSeats = availableSeats,
            OccupiedParkingSlots =
                occupiedParkingSlots,
            TotalRevenue = totalRevenue,
            TotalCustomers = totalCustomers
        };
    }

    public async Task<CustomerDashboardResponse>
        GetCustomerDashboardAsync(
            string customerId,
            CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var upcomingBookings =
            await _dbContext.Bookings
                .CountAsync(
                    booking =>
                        booking.CustomerId ==
                            customerId &&
                        (
                            booking.Status ==
                                HeldStatus ||
                            booking.Status ==
                                ConfirmedStatus
                        ) &&
                        _dbContext.Events.Any(
                            eventEntity =>
                                eventEntity.Id ==
                                    booking.EventId &&
                                eventEntity
                                    .StartDateTimeUtc >
                                    now),
                    cancellationToken);

        var reservedParking =
            await _dbContext.Bookings
                .CountAsync(
                    booking =>
                        booking.CustomerId ==
                            customerId &&
                        (
                            booking.Status ==
                                HeldStatus ||
                            booking.Status ==
                                ConfirmedStatus
                        ) &&
                        booking.ParkingReservation !=
                            null,
                    cancellationToken);

        var recentPayments =
            await _dbContext.Payments
                .CountAsync(
                    payment =>
                        _dbContext.Bookings.Any(
                            booking =>
                                booking.Id ==
                                    payment.BookingId &&
                                booking.CustomerId ==
                                    customerId),
                    cancellationToken);

        var unreadNotifications =
            await _dbContext.Notifications
                .CountAsync(
                    notification =>
                        notification.UserId ==
                            customerId &&
                        !notification.IsRead,
                    cancellationToken);

        return new CustomerDashboardResponse
        {
            UpcomingBookings = upcomingBookings,
            ReservedParking = reservedParking,
            RecentPayments = recentPayments,
            UnreadNotifications =
                unreadNotifications
        };
    }
}
