using EFCoreLibrary.Abstractions.Entity;
using CRMService.Domain.Models.Constants;

namespace CRMService.Domain.Models.CrmEntities
{
    public class Plan : IEntity<Guid>, ICopyable<Plan>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PlanColor { get; set; }
        public string Period { get; set; } = PlanPeriodConstants.MONTH;

        public virtual ICollection<PlanSetting> PlanSettings { get; set; } = new List<PlanSetting>();
        public virtual ICollection<PlanColorScheme> PlanColorSchemes { get; set; } = new List<PlanColorScheme>();

        public void CopyData(Plan entity)
        {
            Name = entity.Name;
            PlanColor = entity.PlanColor;
            Period = entity.Period;
        }
    }
}



