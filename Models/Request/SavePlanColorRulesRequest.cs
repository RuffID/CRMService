using CRMService.Models.Dto.CrmEntities;

namespace CRMService.Models.Request
{
    public class SavePlanColorSchemesRequest
    {
        public Guid PlanId { get; set; }
        public List<PlanColorSchemeDto> Items { get; set; } = new();
    }
}