using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Menu;

namespace depiR5gp.Configurations.Menu;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("Dishes");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(d => d.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(d => d.CuisineId).HasColumnName("cuisine_id").IsRequired();
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(d => d.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
        builder.Property(d => d.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(d => d.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);
        builder.Property(d => d.IsArchived).HasColumnName("is_archived").HasDefaultValue(false);

        builder.HasOne(d => d.ChefProfile)
            .WithMany(c => c.Dishes)
            .HasForeignKey(d => d.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Category)
            .WithMany(c => c.Dishes)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Cuisine)
            .WithMany(c => c.Dishes)
            .HasForeignKey(d => d.CuisineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

