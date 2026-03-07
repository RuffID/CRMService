using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class CompanyCategoryOkdeskConfigure : IEntityTypeConfiguration<CompanyCategory>
    {
        public void Configure(EntityTypeBuilder<CompanyCategory> builder)
        {
            builder.ToTable("company_categories");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Color).HasColumnName("color");
            builder.Ignore(x => x.Companies);
        }
    }
}