using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Configurations.Chefs;

public class ChefDocumentConfiguration : IEntityTypeConfiguration<ChefDocument>
{
    public void Configure(EntityTypeBuilder<ChefDocument> builder)
    {
        builder.ToTable("ChefDocuments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ChefProfileId).HasColumnName("chef_profile_id").IsRequired();
        builder.Property(d => d.DocType).HasColumnName("doc_type").HasMaxLength(50).IsRequired();
        builder.Property(d => d.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
        builder.Property(d => d.UploadedAt).HasColumnName("uploaded_at").HasColumnType("datetime2");

        builder.HasOne(d => d.ChefProfile)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

