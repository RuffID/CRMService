using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class EquipmentParameterFieldTypeJsonConverter : JsonConverter<EquipmentParameterFieldType?>
    {
        public override EquipmentParameterFieldType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.Number)
            {
                int fieldTypeValue = reader.GetInt32();
                return Enum.IsDefined(typeof(EquipmentParameterFieldType), fieldTypeValue)
                    ? (EquipmentParameterFieldType)fieldTypeValue
                    : null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? fieldTypeRaw = reader.GetString();
                return fieldTypeRaw switch
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

            throw new JsonException($"Unsupported JSON token for {nameof(EquipmentParameterFieldType)}: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, EquipmentParameterFieldType? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue((int)value.Value);
        }
    }
}
