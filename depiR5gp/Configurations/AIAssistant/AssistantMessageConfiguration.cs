using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.AIAssistant;

namespace depiR5gp.Configurations.AIAssistant;

public class AssistantMessageConfiguration : IEntityTypeConfiguration<AssistantMessage>
{
    public void Configure(EntityTypeBuilder<AssistantMessage> builder)
    {
        builder.ToTable("AssistantMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.AssistantConversationId).HasColumnName("assistant_conversation_id").IsRequired();
        builder.Property(m => m.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Content).HasColumnName("content").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(m => m.AssistantConversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.AssistantConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

