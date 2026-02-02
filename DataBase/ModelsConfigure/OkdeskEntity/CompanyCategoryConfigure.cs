using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class CompanyCategoryConfigure : IEntityTypeConfiguration<CompanyCategory>
    {
        public void Configure(EntityTypeBuilder<CompanyCategory> builder)
        {
            builder.ToTable("CompanyCategory");            

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(50);

            builder.Property(e => e.Color)
                .HasMaxLength(30);

            builder.Property(e => e.Name)
                .HasMaxLength(100);
        }
    }
}
