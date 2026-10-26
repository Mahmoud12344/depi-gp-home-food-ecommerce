using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Orders;

namespace depiR5gp.Configurations.Orders;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(h => h.ChangedByUserId).HasColumnName("changed_by_user_id");
        builder.Property(h => h.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(h => h.ChangedAt).HasColumnName("changed_at").HasColumnType("datetime2");

        builder.HasOne(h => h.Order)
            .WithMany(o => o.StatusHistories)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany(u => u.OrderStatusHistories)
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

