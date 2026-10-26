using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Messaging;

namespace depiR5gp.Configurations.Messaging;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(m => m.SenderUserId).HasColumnName("sender_user_id").IsRequired();
        builder.Property(m => m.Text).HasColumnName("text").HasMaxLength(2000).IsRequired();
        builder.Property(m => m.SentAt).HasColumnName("sent_at").HasColumnType("datetime2");
        builder.Property(m => m.IsRead).HasColumnName("is_read").HasDefaultValue(false);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SenderUser)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

