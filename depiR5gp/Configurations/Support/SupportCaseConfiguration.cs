using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Support;

namespace depiR5gp.Configurations.Support;

public class SupportCaseConfiguration : IEntityTypeConfiguration<SupportCase>
{
    public void Configure(EntityTypeBuilder<SupportCase> builder)
    {
        builder.ToTable("SupportCases");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.OrderId).HasColumnName("order_id");
        builder.Property(s => s.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(4000).IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("Open");
        builder.Property(s => s.RequesterRole).HasColumnName("requester_role").HasMaxLength(20).IsRequired();
        builder.Property(s => s.AwaitingSupport).HasColumnName("awaiting_support").HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");
        builder.Property(s => s.LastActivityAt).HasColumnName("last_activity_at").HasColumnType("datetime2");
        builder.Property(s => s.ClosedAt).HasColumnName("closed_at").HasColumnType("datetime2");

        builder.HasOne(s => s.User)
            .WithMany(u => u.SupportCases)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.SupportCases)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

