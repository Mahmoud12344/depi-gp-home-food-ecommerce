using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Support;

namespace depiR5gp.Configurations.Support;

public class CaseMessageConfiguration : IEntityTypeConfiguration<CaseMessage>
{
    public void Configure(EntityTypeBuilder<CaseMessage> builder)
    {
        builder.ToTable("CaseMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.SupportCaseId).HasColumnName("support_case_id").IsRequired();
        builder.Property(m => m.AuthorUserId).HasColumnName("author_user_id").IsRequired();
        builder.Property(m => m.AuthorRole).HasColumnName("author_role").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Text).HasColumnName("text").HasMaxLength(4000).IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2");

        builder.HasOne(m => m.SupportCase)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SupportCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.AuthorUser)
            .WithMany(u => u.CaseMessages)
            .HasForeignKey(m => m.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

