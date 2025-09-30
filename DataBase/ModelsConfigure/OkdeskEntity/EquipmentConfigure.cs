using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class EquipmentConfigure : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("equipment");            

            builder.HasIndex(e => e.CompanyId, "companyId_idx");

            builder.HasIndex(e => e.KindId, "kindId_idx");

            builder.HasIndex(e => e.MaintenanceEntitiesId, "maintenanceEntitiesId_idx");

            builder.HasIndex(e => e.ManufacturerId, "manufacturerEquipId_idx");

            builder.HasIndex(e => e.ModelId, "modelEquipId_idx");

            builder.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");

            builder.Property(e => e.CompanyId).HasColumnName("companyId");

            builder.Property(e => e.InventoryNumber).HasMaxLength(300).HasColumnName("inventory_number");

            builder.Property(e => e.KindId).HasColumnName("kindId");

            builder.Property(e => e.MaintenanceEntitiesId).HasColumnName("maintenanceEntitiesId");

            builder.Property(e => e.ManufacturerId).HasColumnName("manufacturerId");

            builder.Property(e => e.ModelId).HasColumnName("modelId");

            builder.Property(e => e.SerialNumber).HasMaxLength(300).HasColumnName("serial_number");

            builder.HasOne(d => d.Company).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equipmentCompanyId");

            builder.HasOne(d => d.Kind).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("kindEquipId");

            builder.HasOne(d => d.MaintenanceEntities).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.MaintenanceEntitiesId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equipmentMaintenanceEntitiesId");

            builder.HasOne(d => d.Manufacturer).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("manufacturerEquipId");

            builder.HasOne(d => d.Model).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Parameters)
                .WithOne(p => p.Equipment)
                .HasForeignKey(p => p.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
