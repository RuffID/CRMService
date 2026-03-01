using CRMService.Models.Dto.OkdeskEntity;

namespace CRMService.Models.Dto.CrmEntities
{
    public class EmployeePlanRowDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? PlanValue { get; set; }
        public List<GroupDto> Groups { get; set; } = new();
    }
}