using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Models.OkdeskSource
{
    public class OkdeskKindParameterRecord
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        public EquipmentParameterFieldType? FieldType { get; set; }
    }
}
