namespace CRMService.Contracts.Models.Dto.Settings
{
    public class ReportBackgroundDto
    {
        public required string DisplayName { get; set; }

        public long SizeBytes { get; set; }

        public DateTime UploadedAtUtc { get; set; }
    }
}
