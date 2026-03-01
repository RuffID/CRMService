namespace CRMService.Models.Dto.CrmEntities
{
    public class PlanDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PlanColor { get; set; }
    }
}