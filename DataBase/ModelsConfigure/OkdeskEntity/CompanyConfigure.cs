using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class CompanyConfigure : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("company");            

            builder.HasIndex(e => e.CategoryId, "categoryId_idx");

            builder.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");

            builder.Property(e => e.Active).HasColumnName("active");

            builder.Property(e => e.AdditionalName).HasMaxLength(400).HasColumnName("additional_name");

            builder.Property(e => e.CategoryId).HasColumnName("categoryId");

            builder.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");

            builder.HasOne(d => d.Category).WithMany(p => p.Companies)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}