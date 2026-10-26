using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Configurations.Chefs;

public class ChefWorkingHoursConfiguration : IEntityTypeConfiguration<ChefWorkingHours>
{
    public void Configure(EntityTypeBuilder<ChefWorkingHours> builder)
    {
        builder.ToTable("ChefWorkingHours");

        builder.HasKey(w => new { w.ChefProfileId, w.DayOfWeek });
        builder.Property(w => w.ChefProfileId).HasColumnName("chef_profile_id");
        builder.Property(w => w.DayOfWeek).HasColumnName("day_of_week");
        builder.Property(w => w.OpenTime).HasColumnName("open_time").HasColumnType("time");
        builder.Property(w => w.CloseTime).HasColumnName("close_time").HasColumnType("time");

        builder.HasOne(w => w.ChefProfile)
            .WithMany(c => c.WorkingHours)
            .HasForeignKey(w => w.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

