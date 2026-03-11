namespace CRMService.Domain.Models.OkdeskEntity
{
    public enum EquipmentParameterFieldType
    {
        String = 0,
        Date = 1,
        DateTime = 2,
        Checkbox = 3,
        SingleSelect = 4,
        MultiSelect = 5
    }

    public static class EquipmentParameterFieldTypeMapper
    {
        public static EquipmentParameterFieldType? Map(string? fieldTypeRaw)
        {
            return fieldTypeRaw switch
            {
                "ftstring" => EquipmentParameterFieldType.String,
                "ftdate" => EquipmentParameterFieldType.Date,
                "ftdatetime" => EquipmentParameterFieldType.DateTime,
                "ftcheckbox" => EquipmentParameterFieldType.Checkbox,
                "ftselect" => EquipmentParameterFieldType.SingleSelect,
                "ftmultiselect" => EquipmentParameterFieldType.MultiSelect,
                null => null,
                _ => null
            };
        }
    }
}
