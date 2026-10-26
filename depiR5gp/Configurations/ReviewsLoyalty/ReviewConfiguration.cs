using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.ReviewsLoyalty;

namespace depiR5gp.Configurations.ReviewsLoyalty;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(r => r.Rating).HasColumnName("rating").IsRequired();
        builder.Property(r => r.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(r => r.ModerationStatus).HasColumnName("moderation_status").HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasIndex(r => r.OrderId).IsUnique();

        builder.HasOne(r => r.Order)
            .WithOne(o => o.Review)
            .HasForeignKey<Review>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ChefProfile)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

