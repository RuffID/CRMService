using System.Text.Json.Serialization;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class TimeEntries
    {
        [JsonPropertyName("spent_time_total")]
        public double Spent_time_total { get; set; }

        [JsonPropertyName("time_entries")]
        public TimeEntry[]? Time_Entries { get; set; } = [];
    }
}