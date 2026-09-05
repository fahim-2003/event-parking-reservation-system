using EventParking.API.Data;
using EventParking.API.DTOs.Payments;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class PaymentService : IPaymentService
{
    private const string PendingStatus = "Pending";
    private const string CompletedStatus = "Completed";

    private readonly AppDbContext _dbContext;

    public PaymentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var bookingExists = await _dbContext.Bookings
            .AnyAsync(
                b => b.Id == request.BookingId,
                cancellationToken);

        if (!bookingExists)
        {
            throw new ArgumentException(
                "Booking does not exist.");
        }

        var existingPayment = await _dbContext.Payments
            .AnyAsync(
                p => p.BookingId == request.BookingId &&
                     p.Status == CompletedStatus,
                cancellationToken);

        if (existingPayment)
        {
            throw new InvalidOperationException(
                "Payment already completed.");
        }

        var payment = new Payment
        {
            BookingId = request.BookingId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PendingStatus
        };

        _dbContext.Payments.Add(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentResponse
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }
}
