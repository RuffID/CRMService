namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class EquipmentDetailsDto
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyCategoryName { get; set; } = string.Empty;

        public string CompanyCategoryColor { get; set; } = string.Empty;

        public string MaintenanceEntityName { get; set; } = string.Empty;

        public List<EquipmentParameterDto> Parameters { get; set; } = new();

        public List<EquipmentDetailsFieldDto> Fields { get; set; } = new();
    }
}
