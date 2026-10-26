using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Cart;

namespace depiR5gp.Configurations.Cart;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.CartId).HasColumnName("cart_id").IsRequired();
        builder.Property(i => i.DishId).HasColumnName("dish_id").IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").HasDefaultValue(1);
        builder.Property(i => i.Note).HasColumnName("note").HasMaxLength(500);

        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Dish)
            .WithMany(d => d.CartItems)
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

