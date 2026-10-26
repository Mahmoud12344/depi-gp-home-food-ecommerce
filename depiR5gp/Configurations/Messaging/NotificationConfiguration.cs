using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Messaging;

namespace depiR5gp.Configurations.Messaging;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        builder.Property(n => n.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Link).HasColumnName("link").HasMaxLength(500);
        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

