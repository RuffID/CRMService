namespace CRMService.Models.Dto.OkdeskEntity
{
    public class MaintenanceEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool Active { get; set; }
    }
}
