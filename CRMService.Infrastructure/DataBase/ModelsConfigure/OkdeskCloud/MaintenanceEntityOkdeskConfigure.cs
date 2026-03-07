using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class MaintenanceEntityOkdeskConfigure : IEntityTypeConfiguration<MaintenanceEntity>
    {
        public void Configure(EntityTypeBuilder<MaintenanceEntity> builder)
        {
            builder.ToTable("company_maintenance_entities");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property<int>("InternalId").HasColumnName("id");
            builder.HasAlternateKey("InternalId");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Active).HasColumnName("active");
            builder.Ignore(x => x.CompanyId);

            builder.Property<int?>("CompanyInternalId").HasColumnName("company_id");

            builder.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey("CompanyInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Ignore(x => x.Address);
            builder.Ignore(x => x.Equipment);
            builder.Ignore(x => x.Issues);
        }
    }
}
