using CRMService.Contracts.Models.Dto.CrmEntities;

namespace CRMService.Contracts.Models.Request
{
    public class SavePlanSettingsRequest
    {
        public List<PlanSettingDto> Items { get; set; } = new();
    }
}



