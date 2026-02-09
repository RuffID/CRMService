using CRMService.Abstractions.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.OkdeskEntity
{
    public class TimeEntry : IEntity<int>, ICopyable<TimeEntry>
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [JsonProperty("spent_time")]
        public double SpentTime { get; set; }

        public int IssueId { get; set; }

        [JsonProperty("logged_at")]
        public DateTime LoggedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Employee? Employee { get; set; }

        public virtual Issue? Issue { get; set; }

        public void CopyData(TimeEntry item)
        {
            EmployeeId = item.EmployeeId;
            SpentTime = item.SpentTime;
            IssueId = item.IssueId;
            LoggedAt = item.LoggedAt;
            CreatedAt = item.CreatedAt;
        }
    }
}