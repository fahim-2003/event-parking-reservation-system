using EventParking.API.Data;
using EventParking.API.DTOs.Payments;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenBookingExists_CreatesPayment()
    {
        await using var dbContext = CreateDbContext();

        var booking = new Booking
        {
            EventId = 1,
            CustomerId = "customer-1",
            Status = "Held",
            PaymentStatus = "Pending"
        };

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        var result = await service.CreateAsync(
            new CreatePaymentRequest
            {
                BookingId = booking.Id,
                Amount = 2500,
                PaymentMethod = "Card"
            });

        Assert.Equal(
            booking.Id,
            result.BookingId);

        Assert.Equal(
            2500,
            result.Amount);

        Assert.Equal(
            "Pending",
            result.Status);

        Assert.Single(
            await dbContext.Payments.ToListAsync());
    }


    [Fact]
    public async Task CreateAsync_WhenCompletedPaymentExists_ThrowsConflict()
    {
        await using var dbContext = CreateDbContext();

        var booking = new Booking
        {
            EventId = 1,
            CustomerId = "customer-1",
            Status = "Confirmed"
        };

        dbContext.Bookings.Add(booking);

        await dbContext.SaveChangesAsync();

        dbContext.Payments.Add(
            new Payment
            {
                BookingId = booking.Id,
                Amount = 2500,
                PaymentMethod = "Card",
                Status = "Completed"
            });

        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                new CreatePaymentRequest
                {
                    BookingId = booking.Id,
                    Amount = 2500,
                    PaymentMethod = "Card"
                }));
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
