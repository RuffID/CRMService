using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class CompanyConfigure : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Company");            

            builder.HasIndex(e => e.CategoryId, "categoryId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.AdditionalName)
                .HasMaxLength(400);

            builder.Property(e => e.Name)
                .HasMaxLength(200);

            builder.HasOne(d => d.Category)
                .WithMany(p => p.Companies)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


