using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class EquipmentConfigure : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipment");            

            builder.HasIndex(e => e.CompanyId, "companyId_idx");

            builder.HasIndex(e => e.KindId, "kindId_idx");

            builder.HasIndex(e => e.MaintenanceEntitiesId, "maintenanceEntitiesId_idx");

            builder.HasIndex(e => e.ManufacturerId, "manufacturerEquipId_idx");

            builder.HasIndex(e => e.ModelId, "modelEquipId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.InventoryNumber)
                .HasMaxLength(300);

            builder.Property(e => e.SerialNumber)
                .HasMaxLength(300);

            builder.HasOne(d => d.Company)
                .WithMany(p => p.Equipment)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Kind)
                .WithMany(p => p.Equipment)
                .HasForeignKey(d => d.KindId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.MaintenanceEntities)
                .WithMany(p => p.Equipment)
                .HasForeignKey(d => d.MaintenanceEntitiesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Manufacturer)
                .WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Model)
                .WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Parameters)
                .WithOne(p => p.Equipment)
                .HasForeignKey(p => p.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
