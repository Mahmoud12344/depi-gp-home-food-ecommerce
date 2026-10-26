using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Payments;

namespace depiR5gp.Configurations.Payments;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(l => l.OrderId).HasColumnName("order_id");
        builder.Property(l => l.PayoutId).HasColumnName("payout_id");
        builder.Property(l => l.RefundRequestId).HasColumnName("refund_request_id");
        builder.Property(l => l.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
        builder.Property(l => l.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(l => l.ChefProfile)
            .WithMany(c => c.LedgerEntries)
            .HasForeignKey(l => l.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Order)
            .WithMany(o => o.LedgerEntries)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Payout)
            .WithMany(p => p.LedgerEntries)
            .HasForeignKey(l => l.PayoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.RefundRequest)
            .WithMany(r => r.LedgerEntries)
            .HasForeignKey(l => l.RefundRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

