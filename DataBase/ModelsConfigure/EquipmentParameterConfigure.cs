using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class EquipmentParameterConfigure : IEntityTypeConfiguration<EquipmentParameter>
    {
        public void Configure(EntityTypeBuilder<EquipmentParameter> builder)
        {
            builder.ToTable("equipment_parameter");

            builder.HasKey(e => new { e.EquipmentId, e.KindParameterId });

            builder.HasIndex(e => e.EquipmentId, "equipmentId_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameterId_idx");

            builder.Property(e => e.EquipmentId)
                .HasColumnName("equipmentId");

            builder.Property(e => e.KindParameterId)
                .HasColumnName("kindParameterId");

            builder.Property(e => e.Value)
                .HasMaxLength(1000)
                .HasColumnName("value");

            builder.HasOne(d => d.Equipment)
                .WithMany(p => p.Parameters)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.KindParameter)
                .WithMany(p => p.Parameters)
                .HasForeignKey(d => d.KindParameterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
