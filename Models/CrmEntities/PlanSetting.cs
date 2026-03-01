using EFCoreLibrary.Abstractions.Entity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.CrmEntities
{
    public class PlanSetting : ICopyable<PlanSetting>
    {
        public Guid PlanId { get; set; }
        public int EmployeeId { get; set; }
        public int? PlanValue { get; set; }

        public virtual Plan Plan { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;

        public void CopyData(PlanSetting entity)
        {
            PlanValue = entity.PlanValue;
        }
    }
}