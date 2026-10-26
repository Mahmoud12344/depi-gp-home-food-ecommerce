using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Payments;

namespace depiR5gp.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(p => p.StripeReference).HasColumnName("stripe_reference").HasMaxLength(100);
        builder.Property(p => p.UnsettledAmount).HasColumnName("unsettled_amount").HasPrecision(10, 2).HasDefaultValue(0.00m);

        builder.HasIndex(p => p.OrderId).IsUnique();

        builder.HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

