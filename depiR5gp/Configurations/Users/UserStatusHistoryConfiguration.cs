using depiR5gp.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace depiR5gp.Configurations.Users;

public class UserStatusHistoryConfiguration : IEntityTypeConfiguration<UserStatusHistory>
{
    public void Configure(EntityTypeBuilder<UserStatusHistory> builder)
    {
        builder.ToTable("UserStatusHistories", t =>
            t.HasCheckConstraint(
                "CK_UserStatusHistories_status",
                "status IN ('Active', 'Suspended', 'Deleted')"));

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(h => h.ChangedByUserId).HasColumnName("changed_by_user_id");

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.ChangedAt)
            .HasColumnName("changed_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(h => h.Reason)
            .HasColumnName("reason")
            .HasMaxLength(200);

        // Many history rows -> one user (history is kept; users are soft-deleted)
        builder.HasOne(h => h.User)
            .WithMany(u => u.StatusHistory)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Admin who performed the change (Restrict: SQL Server forbids multiple SET NULL/cascade paths)
        builder.HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Makes "get latest status for user" fast
        builder.HasIndex(h => new { h.UserId, h.ChangedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_UserStatusHistories_user_changed");
    }
}
