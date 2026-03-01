namespace CRMService.Contracts.Models.Dto.CrmEntities
{
    public class PlanSettingDto
    {
        public Guid PlanId { get; set; }
        public int EmployeeId { get; set; }
        public int? PlanValue { get; set; }
    }
}