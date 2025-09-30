namespace CRMService.Models.Dto.OkdeskEntity
{
    public class CompanyDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? AdditionalName { get; set; }

        public bool Active { get; set; }
    }
}