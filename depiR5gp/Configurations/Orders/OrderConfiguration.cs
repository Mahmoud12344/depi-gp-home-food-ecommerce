using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Orders;

namespace depiR5gp.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(o => o.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(o => o.PaymentMethod).HasColumnName("payment_method").HasMaxLength(30).HasDefaultValue("Cash");
        builder.Property(o => o.PaymentStatus).HasColumnName("payment_status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(o => o.Reference).HasColumnName("reference").HasMaxLength(50).IsRequired();
        builder.Property(o => o.SubmissionToken).HasColumnName("submission_token").HasMaxLength(100);
        builder.Property(o => o.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2).IsRequired();
        builder.Property(o => o.PointsDiscount).HasColumnName("points_discount").HasPrecision(10, 2).HasDefaultValue(0.00m);
        builder.Property(o => o.Total).HasColumnName("total").HasPrecision(10, 2).IsRequired();
        builder.Property(o => o.CommissionRate).HasColumnName("commission_rate").HasPrecision(5, 4).HasDefaultValue(0.1500m);
        builder.Property(o => o.CommissionAmount).HasColumnName("commission_amount").HasPrecision(10, 2).HasDefaultValue(0.00m);
        builder.Property(o => o.DeliveryAddress).HasColumnName("delivery_address").HasMaxLength(500).IsRequired();
        builder.Property(o => o.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(500);
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasIndex(o => o.Reference).IsUnique();
        builder.HasIndex(o => o.SubmissionToken).IsUnique();

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ChefProfile)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

