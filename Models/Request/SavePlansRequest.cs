using CRMService.Models.Dto.CrmEntities;

namespace CRMService.Models.Request
{
    public class SavePlansRequest
    {
        public List<PlanDto> Items { get; set; } = new();
    }
}