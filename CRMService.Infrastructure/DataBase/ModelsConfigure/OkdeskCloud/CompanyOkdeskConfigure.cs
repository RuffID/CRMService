using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class CompanyOkdeskConfigure : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property<int>("InternalId").HasColumnName("id");
            builder.HasAlternateKey("InternalId");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.AdditionalName).HasColumnName("additional_name");
            builder.Property(x => x.Active).HasColumnName("active");
            builder.Property(x => x.CategoryId).HasColumnName("category_id");

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Ignore(x => x.Equipment);
            builder.Ignore(x => x.Issues);
            builder.Ignore(x => x.MaintenanceEntities);
        }
    }
}