namespace CRMService.Contracts.Models.Dto.CrmEntities
{
    public class PlanDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PlanColor { get; set; }
        public string Period { get; set; } = string.Empty;
    }
}