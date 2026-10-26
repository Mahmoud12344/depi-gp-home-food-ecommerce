using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Orders;

namespace depiR5gp.Configurations.Orders;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => new { i.OrderId, i.LineNo });
        builder.Property(i => i.OrderId).HasColumnName("order_id");
        builder.Property(i => i.LineNo).HasColumnName("line_no");
        builder.Property(i => i.DishId).HasColumnName("dish_id").IsRequired();
        builder.Property(i => i.DishName).HasColumnName("dish_name").HasMaxLength(150).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasPrecision(10, 2).IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").HasDefaultValue(1);
        builder.Property(i => i.Note).HasColumnName("note").HasMaxLength(500);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Dish)
            .WithMany(d => d.OrderItems)
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

