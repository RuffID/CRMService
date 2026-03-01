namespace CRMService.Contracts.Models.Request
{
    public class ReportRequest
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public Guid? PlanId { get; set; }
        public IReadOnlyCollection<int>? EmployeeIds { get; init; }
        public IReadOnlyCollection<int>? StatusIds { get; init; }
        public IReadOnlyCollection<int>? PriorityIds { get; init; }
        public IReadOnlyCollection<int>? TypeIds { get; init; }
        public IReadOnlyCollection<int>? GroupIds { get; init; }

        public bool HideWithoutSolved { get; set; }
        public bool HideWithoutCurrent { get; set; }
        public bool HideWithoutTime { get; set; }

        public bool HasEmployees => EmployeeIds != null && EmployeeIds.Count > 0;
        public bool HasStatus => StatusIds != null && StatusIds.Count > 0;
        public bool HasPriority => PriorityIds != null && PriorityIds.Count > 0;
        public bool HasType => TypeIds != null && TypeIds.Count > 0;
        public bool HasGroups => GroupIds != null && GroupIds.Count > 0;
    }
}


