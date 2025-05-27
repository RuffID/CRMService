using CRMService.Models.Entity;

namespace CRMService.Models.WebHook
{
    public class TimeEntryWebHook
    {
        public int Id { get; set; }
        public double Spent_time { get; set; }
        public EmployeeWebHook? Employee { get; set; }
    }
}
