using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.WebHook
{
    public class TimeEntryWebHook
    {
        public int Id { get; set; }
        public double Spent_time { get; set; }
        public EmployeeWebHook Employee { get; set; } = null!;
        public DateTime Logged_at { get; set; }
    }
}
