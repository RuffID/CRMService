namespace CRMService.Contracts.Models.Dto.Settings
{
    public class ReportBackgroundSettingsDto
    {
        public ReportBackgroundDto? CurrentBackground { get; set; }

        public List<ReportBackgroundDto> Backgrounds { get; set; } = [];
    }
}