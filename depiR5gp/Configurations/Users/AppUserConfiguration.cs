using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Users;

namespace depiR5gp.Configurations.Users;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
         builder.ToTable("AppUsers", t =>
            t.HasCheckConstraint(
                "CK_AppUsers_status",
                "status IN ('Active', 'Suspended', 'Deleted')"));

        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(u => u.PhoneNumber).HasColumnName("phone").HasMaxLength(30);
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(UserStatus.Active);
        
        builder.Property(u => u.PointsBalance).HasColumnName("points_balance").HasDefaultValue(0);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(UserStatus.Active);


        // 1:1 with ChefProfile
        builder.HasOne(u => u.ChefProfile)
            .WithOne(c => c.User)
            .HasForeignKey<depiR5gp.Models.Chefs.ChefProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

