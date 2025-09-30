namespace CRMService.Models.OkdeskEntity
{
    public class TimeEntries
    {
        public double Spent_time_total { get; set; }
        public TimeEntry[]? Time_Entries { get; set; } = [];
    }
}