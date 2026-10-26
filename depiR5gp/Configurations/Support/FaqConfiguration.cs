using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Support;

namespace depiR5gp.Configurations.Support;

public class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(f => f.Question).HasColumnName("question").HasMaxLength(500).IsRequired();
        builder.Property(f => f.Answer).HasColumnName("answer").HasMaxLength(4000).IsRequired();
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2");

        builder.HasOne(f => f.UpdatedByUser)
            .WithMany()
            .HasForeignKey(f => f.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

