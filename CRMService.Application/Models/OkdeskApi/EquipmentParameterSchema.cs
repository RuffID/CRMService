using CRMService.Domain.Models.OkdeskEntity;
using System.Text.Json.Serialization;

namespace CRMService.Application.Models.OkdeskApi
{
    public class EquipmentParameterSchema
    {
        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        [JsonPropertyName("field_type")]
        [JsonConverter(typeof(EquipmentParameterFieldTypeJsonConverter))]
        public EquipmentParameterFieldType? FieldType { get; set; }

        [JsonPropertyName("equipment_kind_codes")]
        public string[] EquipmentKindCodes { get; set; } = [];
    }
}
