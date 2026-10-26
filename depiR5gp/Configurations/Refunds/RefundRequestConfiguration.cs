using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Refunds;

namespace depiR5gp.Configurations.Refunds;

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("RefundRequests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Requested");
        builder.Property(r => r.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
        builder.Property(r => r.RequestedAt).HasColumnName("requested_at").HasColumnType("datetime2");
        builder.Property(r => r.Decision).HasColumnName("decision").HasMaxLength(30);
        builder.Property(r => r.DecisionReason).HasColumnName("decision_reason").HasMaxLength(1000);
        builder.Property(r => r.DecidedAt).HasColumnName("decided_at").HasColumnType("datetime2");
        builder.Property(r => r.StripeRefundRef).HasColumnName("stripe_refund_ref").HasMaxLength(100);

        builder.HasOne(r => r.Order)
            .WithMany(o => o.RefundRequests)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefundRequests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

