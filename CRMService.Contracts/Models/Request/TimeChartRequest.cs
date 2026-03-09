namespace CRMService.Contracts.Models.Request
{
    public class TimeChartRequest
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string TimeAxis { get; set; } = string.Empty;

        public string Granularity { get; set; } = string.Empty;

        public IReadOnlyCollection<int>? EmployeeIds { get; init; }

        public IReadOnlyCollection<int>? GroupIds { get; init; }

        public bool HasEmployees => EmployeeIds != null && EmployeeIds.Count > 0;

        public bool HasGroups => GroupIds != null && GroupIds.Count > 0;
    }
}