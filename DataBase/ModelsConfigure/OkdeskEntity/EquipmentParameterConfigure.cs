using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
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

            /*builder.Property(e => e.Value)
                .HasMaxLength(1000)
                .HasColumnName("value");*/

            // сравнивать object по JSON
            ValueComparer<object?> jsonComparer = new ((a, b) =>
                a == null && b == null ? true :
                a == null || b == null ? false :
                JToken.DeepEquals(JToken.FromObject(a), JToken.FromObject(b)),

                v => v == null ? 0 : JToken.FromObject(v).ToString(Formatting.None).GetHashCode(),

                v => v == null ? null : JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(v, Formatting.None))
            );

            builder.Property(e => e.Value)
            .HasConversion(
                v => v == null ? null : JsonConvert.SerializeObject(v, Formatting.None),
                v => v == null ? null : JsonConvert.DeserializeObject<object>(v)
            )
            .HasColumnType("nvarchar(max)")
            .HasColumnName("value")
            .Metadata.SetValueComparer(jsonComparer);

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
