using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Cart;

namespace depiR5gp.Configurations.Cart;

public class CartConfiguration : IEntityTypeConfiguration<Models.Cart.Cart>
{
    public void Configure(EntityTypeBuilder<Models.Cart.Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.UserId).HasColumnName("user_id");
        builder.Property(c => c.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(c => c.GuestToken).HasColumnName("guest_token").HasMaxLength(100);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.ChefProfile)
            .WithMany(cp => cp.Carts)
            .HasForeignKey(c => c.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

