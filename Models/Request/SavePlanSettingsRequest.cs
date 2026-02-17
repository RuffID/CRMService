using CRMService.Models.Dto.CrmEntities;

namespace CRMService.Models.Request
{
    public class SavePlanSettingsRequest
    {
        public List<PlanSettingDto> Items { get; set; } = new();
    }
}
