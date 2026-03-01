using CRMService.Contracts.Models.Dto.CrmEntities;

namespace CRMService.Contracts.Models.Request
{
    public class SavePlansRequest
    {
        public List<PlanDto> Items { get; set; } = new();
    }
}


