using CRMService.Contracts.Models.Dto.CrmEntities;

namespace CRMService.Contracts.Models.Request
{
    public class SavePlanColorSchemesRequest
    {
        public Guid PlanId { get; set; }
        public List<PlanColorSchemeDto> Items { get; set; } = new();
    }
}


