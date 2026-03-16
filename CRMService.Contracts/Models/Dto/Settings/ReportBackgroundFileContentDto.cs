namespace CRMService.Contracts.Models.Dto.Settings
{
    public class ReportBackgroundFileContentDto
    {
        public required string FileName { get; set; }

        public required string ContentType { get; set; }

        public required byte[] Content { get; set; }
    }
}