using CRMService.Contracts.Models.Dto.OkdeskEntity;

namespace CRMService.Application.Models.WebHook
{
    public class MaintenanceEntityWebHook
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
        public CompanyDto Company { get; set; } = null!;
    }
}



