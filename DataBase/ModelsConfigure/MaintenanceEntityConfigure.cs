using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class MaintenanceEntityConfigure : IEntityTypeConfiguration<MaintenanceEntity>
    {
        public void Configure(EntityTypeBuilder<MaintenanceEntity> builder)
        {
            builder.ToTable("maintenance_entity");

            builder.HasIndex(e => e.CompanyId, "companyId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            builder.Property(e => e.Active).HasColumnName("active");

            builder.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");

            builder.Property(e => e.CompanyId).HasColumnName("companyId");

            builder.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");

            builder.HasOne(d => d.Company).WithMany(p => p.MaintenanceEntities)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("companyId");
        }
    }
}
