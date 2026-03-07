using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class EquipmentOkdeskConfigure : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("equipments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property(x => x.SerialNumber).HasColumnName("serial_number");
            builder.Property(x => x.InventoryNumber).HasColumnName("inventory_number");
            builder.Property<string?>("ParametersJson").HasColumnName("parameters");            

            builder.Property<int?>("CompanyInternalId").HasColumnName("company_id");
            builder.Property<int?>("MaintenanceEntityInternalId").HasColumnName("maintenance_entity_id");
            builder.Property(x => x.KindId).HasColumnName("equipment_kind_id");
            builder.Property(x => x.ManufacturerId).HasColumnName("equipment_manufacturer_id");
            builder.Property(x => x.ModelId).HasColumnName("equipment_model_id");

            builder.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey("CompanyInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.MaintenanceEntities)
                .WithMany()
                .HasForeignKey("MaintenanceEntityInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Kind)
                .WithMany()
                .HasForeignKey(x => x.KindId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Manufacturer)
                .WithMany()
                .HasForeignKey(x => x.ManufacturerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Model)
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Ignore(x => x.CompanyId);
            builder.Ignore(x => x.MaintenanceEntitiesId);
            builder.Ignore(x => x.Parameters);
        }
    }
}