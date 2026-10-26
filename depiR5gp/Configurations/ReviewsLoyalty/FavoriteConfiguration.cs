using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.ReviewsLoyalty;

namespace depiR5gp.Configurations.ReviewsLoyalty;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(f => f.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();

        builder.HasIndex(f => new { f.UserId, f.ChefProfileId }).IsUnique();

        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ChefProfile)
            .WithMany(c => c.Favorites)
            .HasForeignKey(f => f.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

