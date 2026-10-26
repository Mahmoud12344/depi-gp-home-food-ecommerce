using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Orders;

namespace depiR5gp.Configurations.Orders;

public class OrderItemOptionConfiguration : IEntityTypeConfiguration<OrderItemOption>
{
    public void Configure(EntityTypeBuilder<OrderItemOption> builder)
    {
        builder.ToTable("OrderItemOptions");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(o => o.OrderItemLineNo).HasColumnName("order_item_line_no").IsRequired();
        builder.Property(o => o.GroupName).HasColumnName("group_name").HasMaxLength(100).IsRequired();
        builder.Property(o => o.OptionName).HasColumnName("option_name").HasMaxLength(100).IsRequired();
        builder.Property(o => o.ExtraPrice).HasColumnName("extra_price").HasPrecision(10, 2).HasDefaultValue(0.00m);

        builder.HasOne(o => o.OrderItem)
            .WithMany(i => i.Options)
            .HasForeignKey(o => new { o.OrderId, o.OrderItemLineNo })
            .OnDelete(DeleteBehavior.Cascade);
    }
}

