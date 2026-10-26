using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.AIAssistant;

namespace depiR5gp.Configurations.AIAssistant;

public class AssistantUsageConfiguration : IEntityTypeConfiguration<AssistantUsage>
{
    public void Configure(EntityTypeBuilder<AssistantUsage> builder)
    {
        builder.ToTable("AssistantUsages");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(u => u.UsageDate).HasColumnName("usage_date").HasColumnType("date");
        builder.Property(u => u.RequestCount).HasColumnName("request_count").HasDefaultValue(0);

        builder.HasIndex(u => new { u.UserId, u.UsageDate }).IsUnique();

        builder.HasOne(u => u.User)
            .WithMany(usr => usr.AssistantUsages)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

