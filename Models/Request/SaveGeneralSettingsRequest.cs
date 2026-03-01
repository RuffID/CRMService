using CRMService.Models.Dto.CrmEntities;

namespace CRMService.Models.Request
{
    public class SaveGeneralSettingsRequest
    {
        public GeneralSettingsDto Item { get; set; } = new();
    }
}