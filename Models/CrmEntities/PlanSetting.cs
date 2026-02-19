using CRMService.Abstractions.Entity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.CrmEntities
{
    public class PlanSetting : IEntity<Guid>, ICopyable<PlanSetting>
    {
        public Guid Id { get; set; }
        public int EmployeeId { get; set; }
        public int? MonthPlan { get; set; }
        public int? DayPlan { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public void CopyData(PlanSetting entity)
        {
            MonthPlan = entity.MonthPlan;
            DayPlan = entity.DayPlan;
        }
    }
}