namespace CRMService.Models.Dto.CrmEntities
{
    public class PlanColorSchemeDto
    {
        public Guid? Id { get; set; }
        public Guid PlanId { get; set; }
        public int FromPercent { get; set; }
        public int? ToPercent { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}