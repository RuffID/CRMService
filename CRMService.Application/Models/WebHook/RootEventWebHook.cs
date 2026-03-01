using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Models.WebHook
{
    public class RootEventWebHook
    {
        public EventWebHook? Event { get; set; }
        public IssueWebHook? Issue { get; set; }
        public Company? Company { get; set; }
        public MaintenanceEntityWebHook? Service_aim { get; set; }
        public Equipment? Equipment { get; set; }
    }
}


