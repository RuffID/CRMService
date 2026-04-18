namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class EquipmentListItemDto
    {
        public int Id { get; set; }

        public string TypeName { get; set; } = string.Empty;

        public string ManufacturerName { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public string InventoryNumber { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyCategoryColor { get; set; } = string.Empty;

        public string MaintenanceEntityName { get; set; } = string.Empty;

        public List<EquipmentParameterDto> Parameters { get; set; } = new();
    }
}
