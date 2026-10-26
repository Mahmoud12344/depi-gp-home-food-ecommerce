using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.ReviewsLoyalty;

namespace depiR5gp.Configurations.ReviewsLoyalty;

public class PointsLedgerConfiguration : IEntityTypeConfiguration<PointsLedger>
{
    public void Configure(EntityTypeBuilder<PointsLedger> builder)
    {
        builder.ToTable("PointsLedgers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.OrderId).HasColumnName("order_id");
        builder.Property(p => p.Points).HasColumnName("points").IsRequired();
        builder.Property(p => p.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(p => p.User)
            .WithMany(u => u.PointsLedgerEntries)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.PointsLedgerEntries)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

