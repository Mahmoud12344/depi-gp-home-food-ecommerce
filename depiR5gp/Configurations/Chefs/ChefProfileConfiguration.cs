using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Configurations.Chefs;

public class ChefProfileConfiguration : IEntityTypeConfiguration<ChefProfile>
{
    public void Configure(EntityTypeBuilder<ChefProfile> builder)
    {
        builder.ToTable("ChefProfiles");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.KitchenName).HasColumnName("kitchen_name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Bio).HasColumnName("bio").HasMaxLength(1000);
        builder.Property(c => c.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(300);
        builder.Property(c => c.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(c => c.ApprovalNotes).HasColumnName("approval_notes").HasMaxLength(1000);
        builder.Property(c => c.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(1000);
        builder.Property(c => c.HasAcceptedTerms).HasColumnName("has_accepted_terms").HasDefaultValue(false);
        builder.Property(c => c.CommissionRate).HasColumnName("commission_rate").HasPrecision(5, 4);

        builder.HasIndex(c => c.UserId).IsUnique();

        builder.HasOne(c => c.User)
            .WithOne(u => u.ChefProfile)
            .HasForeignKey<ChefProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

