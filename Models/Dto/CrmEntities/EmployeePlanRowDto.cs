namespace CRMService.Models.Dto.CrmEntities
{
    public class EmployeePlanRowDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? MonthPlan { get; set; }
        public int? DayPlan { get; set; }
    }
}
