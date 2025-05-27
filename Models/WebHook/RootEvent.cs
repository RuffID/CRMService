using CRMService.Models.Entity;

namespace CRMService.Models.WebHook
{
    public class RootEvent
    {
        public Event? Event { get; set; }
        public IssueJSON? Issue { get; set; }
        public Company? Company { get; set; }
        public MaintenanceEntityWebHook? Service_aim { get; set; }
        public Equipment? Equipment { get; set; }
    }
}