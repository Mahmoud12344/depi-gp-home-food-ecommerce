using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Messaging;

namespace depiR5gp.Configurations.Messaging;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.CustomerUserId).HasColumnName("customer_user_id").IsRequired();
        builder.Property(c => c.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasIndex(c => new { c.CustomerUserId, c.ChefProfileId }).IsUnique();

        builder.HasOne(c => c.CustomerUser)
            .WithMany(u => u.CustomerConversations)
            .HasForeignKey(c => c.CustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ChefProfile)
            .WithMany(cp => cp.Conversations)
            .HasForeignKey(c => c.ChefProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

