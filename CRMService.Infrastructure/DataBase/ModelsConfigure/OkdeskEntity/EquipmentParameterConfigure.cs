using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class EquipmentParameterConfigure : IEntityTypeConfiguration<EquipmentParameter>
    {
        public void Configure(EntityTypeBuilder<EquipmentParameter> builder)
        {
            builder.ToTable("EquipmentParameter");

            builder.HasKey(e => new { e.EquipmentId, e.KindParameterId });

            builder.HasIndex(e => e.EquipmentId, "equipmentId_idx");

            builder.HasIndex(e => e.KindParameterId, "kindParameterId_idx");

            ValueComparer<object?> jsonComparer = new(
                (a, b) => JsonValuesEqual(a, b),
                v => JsonValueHash(v),
                v => JsonValueSnapshot(v)
            );

            builder.Property(e => e.Value)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v),
                    v => v == null ? null : JsonSerializer.Deserialize<object>(v)
                )
                .HasColumnType("nvarchar(max)")
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

        private static bool JsonValuesEqual(object? left, object? right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            JsonElement leftJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(left));
            JsonElement rightJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(right));

            return JsonElement.DeepEquals(leftJson, rightJson);
        }

        private static int JsonValueHash(object? value)
        {
            return value == null ? 0 : JsonSerializer.Serialize(value).GetHashCode();
        }

        private static object? JsonValueSnapshot(object? value)
        {
            return value == null ? null : JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(value));
        }
    }
}