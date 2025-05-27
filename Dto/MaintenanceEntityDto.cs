namespace CRMService.Dto
{
    public class MaintenanceEntityDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public bool? Active { get; set; }

        public int? CompanyId { get; set; }
    }
}
