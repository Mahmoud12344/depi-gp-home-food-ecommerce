using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Platform;

namespace depiR5gp.Configurations.Platform;

public class PlatformSettingsConfiguration : IEntityTypeConfiguration<PlatformSettings>
{
    public void Configure(EntityTypeBuilder<PlatformSettings> builder)
    {
        builder.ToTable("PlatformSettings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(s => s.CommissionRate).HasColumnName("commission_rate").HasPrecision(5, 4).HasDefaultValue(0.1500m);
        builder.Property(s => s.RefundWindowHours).HasColumnName("refund_window_hours").HasDefaultValue(24);
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2");

        builder.HasOne(s => s.UpdatedByUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

