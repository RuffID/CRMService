using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskEntity
{
    public class KindsParameterConfigure : IEntityTypeConfiguration<KindsParameter>
    {
        private static readonly ValueConverter<EquipmentParameterFieldType?, string?> FIELD_TYPE_CONVERTER = new(
            fieldType => fieldType.HasValue ? fieldType.Value.ToString() : null,
            fieldType => MapFieldType(fieldType));

        public void Configure(EntityTypeBuilder<KindsParameter> builder)
        {
            builder.ToTable("KindsParameters");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(30);

            builder.Property(e => e.Name)
                .HasMaxLength(200);

            builder.Property(e => e.FieldType)
                .HasConversion(FIELD_TYPE_CONVERTER);
        }

        private static EquipmentParameterFieldType? MapFieldType(string? fieldType)
        {
            if (string.IsNullOrWhiteSpace(fieldType))
                return null;

            if (Enum.TryParse<EquipmentParameterFieldType>(fieldType, true, out EquipmentParameterFieldType parsedFieldType))
                return parsedFieldType;

            return fieldType switch
            {
                "ftstring" => EquipmentParameterFieldType.String,
                "ftdate" => EquipmentParameterFieldType.Date,
                "ftdatetime" => EquipmentParameterFieldType.DateTime,
                "ftcheckbox" => EquipmentParameterFieldType.Checkbox,
                "ftselect" => EquipmentParameterFieldType.SingleSelect,
                "ftmultiselect" => EquipmentParameterFieldType.MultiSelect,
                _ => null
            };
        }
    }
}
