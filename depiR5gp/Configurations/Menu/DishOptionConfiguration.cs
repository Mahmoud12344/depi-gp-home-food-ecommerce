using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Menu;

namespace depiR5gp.Configurations.Menu;

public class DishOptionConfiguration : IEntityTypeConfiguration<DishOption>
{
    public void Configure(EntityTypeBuilder<DishOption> builder)
    {
        builder.ToTable("DishOptions");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.DishId).HasColumnName("dish_id").IsRequired();
        builder.Property(o => o.GroupName).HasColumnName("group_name").HasMaxLength(100).IsRequired();
        builder.Property(o => o.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(o => o.ExtraPrice).HasColumnName("extra_price").HasPrecision(10, 2).HasDefaultValue(0.00m);
        builder.Property(o => o.IsRequired).HasColumnName("is_required").HasDefaultValue(false);

        builder.HasOne(o => o.Dish)
            .WithMany(d => d.DishOptions)
            .HasForeignKey(o => o.DishId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

