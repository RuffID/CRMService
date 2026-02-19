using CRMService.Models.Dto.CrmEntities;

namespace CRMService.Models.Request
{
    public class SavePlanColorSchemesRequest
    {
        public List<PlanColorSchemeDto> Items { get; set; } = new();
    }
}
