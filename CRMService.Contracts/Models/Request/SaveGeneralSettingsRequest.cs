using CRMService.Contracts.Models.Dto.CrmEntities;

namespace CRMService.Contracts.Models.Request
{
    public class SaveGeneralSettingsRequest
    {
        public GeneralSettingsDto Item { get; set; } = new();
    }
}


