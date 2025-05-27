using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class ParameterConfigure : IEntityTypeConfiguration<Parameter>
    {
        public void Configure(EntityTypeBuilder<Parameter> builder)
        {
            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.ToTable("parameter");

            builder.HasIndex(e => e.EquipmentId, "equipmentId_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameterId_idx");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            builder.Property(e => e.EquipmentId).HasColumnName("equipmentId");
            builder.Property(e => e.KindParameterId).HasColumnName("kindParameterId");
            builder.Property(e => e.Value)
                .HasConversion(
                    v => Convert.ToString(v),
                    v => v)
                .HasMaxLength(1000)
                .HasColumnName("value");

            builder.HasOne(d => d.Equipment).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("equipmentId");

            builder.HasOne(d => d.KindParameter).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.KindParameterId)
                .HasConstraintName("kindParameterId");
        }
    }
}
