using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class PaymentConfiguration :
    IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.PaymentMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(10, 2);

        builder.Property(payment => payment.CreatedAt)
            .IsRequired();

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(payment => payment.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(payment => payment.BookingId)
            .IsUnique();
    }
}
