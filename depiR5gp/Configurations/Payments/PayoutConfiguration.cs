using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Payments;

namespace depiR5gp.Configurations.Payments;

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("Payouts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(p => p.Reference).HasColumnName("reference").HasMaxLength(100);
        builder.Property(p => p.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        builder.Property(p => p.IsSimulated).HasColumnName("is_simulated").HasDefaultValue(false);

        builder.HasIndex(p => p.IdempotencyKey).IsUnique();

        builder.HasOne(p => p.ChefProfile)
            .WithMany(c => c.Payouts)
            .HasForeignKey(p => p.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

