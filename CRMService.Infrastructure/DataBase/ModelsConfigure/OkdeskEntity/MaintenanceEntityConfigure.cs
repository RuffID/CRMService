using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class MaintenanceEntityConfigure : IEntityTypeConfiguration<MaintenanceEntity>
    {
        public void Configure(EntityTypeBuilder<MaintenanceEntity> builder)
        {
            builder.ToTable("MaintenanceEntity");

            builder.HasIndex(e => e.CompanyId, "companyId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Address)
                .HasMaxLength(300);

            builder.Property(e => e.Name)
                .HasMaxLength(200);

            builder.HasOne(d => d.Company)
                .WithMany(p => p.MaintenanceEntities)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}



