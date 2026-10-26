using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Cart;

namespace depiR5gp.Configurations.Cart;

public class CartItemOptionConfiguration : IEntityTypeConfiguration<CartItemOption>
{
    public void Configure(EntityTypeBuilder<CartItemOption> builder)
    {
        builder.ToTable("CartItemOptions");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.CartItemId).HasColumnName("cart_item_id").IsRequired();
        builder.Property(o => o.DishOptionId).HasColumnName("dish_option_id").IsRequired();

        builder.HasOne(o => o.CartItem)
            .WithMany(i => i.Options)
            .HasForeignKey(o => o.CartItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.DishOption)
            .WithMany(d => d.CartItemOptions)
            .HasForeignKey(o => o.DishOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

