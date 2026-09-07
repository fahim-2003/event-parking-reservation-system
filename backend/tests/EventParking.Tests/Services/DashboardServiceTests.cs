using EventParking.API.Data;
using EventParking.API.Entities;
using EventParking.API.Identity;
using EventParking.API.Services.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetAdminDashboardAsync_ReturnsCorrectMetrics()
    {
        await using var db = CreateDbContext();

        var customerRole =
            new IdentityRole
            {
                Name = "Customer",
                NormalizedName = "CUSTOMER"
            };

        var adminRole =
            new IdentityRole
            {
                Name = "Administrator",
                NormalizedName =
                    "ADMINISTRATOR"
            };

        db.Roles.AddRange(
            customerRole,
            adminRole);

        var customer =
            new ApplicationUser
            {
                Id = "customer-1",
                UserName = "customer@test.local",
                NormalizedUserName =
                    "CUSTOMER@TEST.LOCAL",
                Email = "customer@test.local",
                NormalizedEmail =
                    "CUSTOMER@TEST.LOCAL",
                FullName = "Customer One"
            };

        var admin =
            new ApplicationUser
            {
                Id = "admin-1",
                UserName = "admin@test.local",
                NormalizedUserName =
                    "ADMIN@TEST.LOCAL",
                Email = "admin@test.local",
                NormalizedEmail =
                    "ADMIN@TEST.LOCAL",
                FullName = "Administrator"
            };

        db.Users.AddRange(customer, admin);

        await db.SaveChangesAsync();

        db.UserRoles.AddRange(
            new IdentityUserRole<string>
            {
                UserId = customer.Id,
                RoleId = customerRole.Id
            },
            new IdentityUserRole<string>
            {
                UserId = admin.Id,
                RoleId = adminRole.Id
            });

        var eventEntity =
            new Event
            {
                Name = "Dashboard Event",
                Capacity = 10,
                StartDateTimeUtc =
                    DateTime.UtcNow.AddDays(1),
                EndDateTimeUtc =
                    DateTime.UtcNow.AddDays(1)
                        .AddHours(2)
            };

        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        db.Seats.AddRange(
            new Seat
            {
                EventId = eventEntity.Id,
                RowLabel = "A",
                SeatNumber = 1,
                DisplayLabel = "A1",
                Status = "Available"
            },
            new Seat
            {
                EventId = eventEntity.Id,
                RowLabel = "A",
                SeatNumber = 2,
                DisplayLabel = "A2",
                Status = "Booked"
            });

        db.ParkingSlots.AddRange(
            new ParkingSlot
            {
                EventId = eventEntity.Id,
                Zone = "A",
                SlotNumber = 1,
                Status = "Occupied"
            },
            new ParkingSlot
            {
                EventId = eventEntity.Id,
                Zone = "A",
                SlotNumber = 2,
                Status = "Available"
            });

        var booking =
            new Booking
            {
                BookingNumber = "BK-DASH-1",
                EventId = eventEntity.Id,
                CustomerId = customer.Id,
                Status = "Confirmed",
                PaymentStatus = "Completed"
            };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        db.Payments.Add(
            new Payment
            {
                BookingId = booking.Id,
                Amount = 1500m,
                PaymentMethod = "Card",
                Status = "Completed"
            });

        await db.SaveChangesAsync();

        var service =
            new DashboardService(db);

        var result =
            await service.GetAdminDashboardAsync(
                CancellationToken.None);

        Assert.Equal(1, result.TotalEvents);
        Assert.Equal(1, result.TotalBookings);
        Assert.Equal(1, result.AvailableSeats);
        Assert.Equal(
            1,
            result.OccupiedParkingSlots);
        Assert.Equal(1500m, result.TotalRevenue);

        // Administrator must not be counted.
        Assert.Equal(1, result.TotalCustomers);
    }

    [Fact]
    public async Task GetCustomerDashboardAsync_ExcludesInvalidBookingsAndCountsUnreadNotifications()
    {
        await using var db = CreateDbContext();

        var futureEvent =
            new Event
            {
                Name = "Future Event",
                Capacity = 10,
                StartDateTimeUtc =
                    DateTime.UtcNow.AddDays(2),
                EndDateTimeUtc =
                    DateTime.UtcNow.AddDays(2)
                        .AddHours(2)
            };

        var pastEvent =
            new Event
            {
                Name = "Past Event",
                Capacity = 10,
                StartDateTimeUtc =
                    DateTime.UtcNow.AddDays(-2),
                EndDateTimeUtc =
                    DateTime.UtcNow.AddDays(-2)
                        .AddHours(2)
            };

        db.Events.AddRange(
            futureEvent,
            pastEvent);

        await db.SaveChangesAsync();

        var parking1 =
            new ParkingSlot
            {
                EventId = futureEvent.Id,
                Zone = "A",
                SlotNumber = 1,
                Status = "Held"
            };

        var parking2 =
            new ParkingSlot
            {
                EventId = futureEvent.Id,
                Zone = "A",
                SlotNumber = 2,
                Status = "Available"
            };

        db.ParkingSlots.AddRange(
            parking1,
            parking2);

        await db.SaveChangesAsync();

        var activeBooking =
            new Booking
            {
                BookingNumber = "BK-DASH-ACTIVE",
                EventId = futureEvent.Id,
                CustomerId = "customer-1",
                Status = "Held",
                PaymentStatus = "Pending"
            };

        var cancelledBooking =
            new Booking
            {
                BookingNumber =
                    "BK-DASH-CANCELLED",
                EventId = futureEvent.Id,
                CustomerId = "customer-1",
                Status = "Cancelled",
                PaymentStatus = "Pending"
            };

        var pastBooking =
            new Booking
            {
                BookingNumber = "BK-DASH-PAST",
                EventId = pastEvent.Id,
                CustomerId = "customer-1",
                Status = "Confirmed",
                PaymentStatus = "Completed"
            };

        db.Bookings.AddRange(
            activeBooking,
            cancelledBooking,
            pastBooking);

        await db.SaveChangesAsync();

        db.ParkingReservations.AddRange(
            new ParkingReservation
            {
                BookingId = activeBooking.Id,
                ParkingSlotId = parking1.Id
            },
            new ParkingReservation
            {
                BookingId =
                    cancelledBooking.Id,
                ParkingSlotId = parking2.Id
            });

        db.Payments.Add(
            new Payment
            {
                BookingId = pastBooking.Id,
                Amount = 1000m,
                PaymentMethod = "Card",
                Status = "Completed"
            });

        db.Notifications.AddRange(
            new Notification
            {
                UserId = "customer-1",
                Message = "Unread one",
                IsRead = false
            },
            new Notification
            {
                UserId = "customer-1",
                Message = "Already read",
                IsRead = true
            },
            new Notification
            {
                UserId = "another-customer",
                Message = "Other user",
                IsRead = false
            });

        await db.SaveChangesAsync();

        var service =
            new DashboardService(db);

        var result =
            await service.GetCustomerDashboardAsync(
                "customer-1",
                CancellationToken.None);

        // Only active future booking.
        Assert.Equal(
            1,
            result.UpcomingBookings);

        // Cancelled parking must not count.
        Assert.Equal(
            1,
            result.ReservedParking);

        Assert.Equal(
            1,
            result.RecentPayments);

        Assert.Equal(
            1,
            result.UnreadNotifications);
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
    }
}
