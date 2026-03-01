using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Domain.Models.CrmEntities
{
    public class GeneralSettings : IEntity<Guid>, ICopyable<GeneralSettings>
    {
        public Guid Id { get; set; }
        public int PlanSwitchSeconds { get; set; }

        public void CopyData(GeneralSettings entity)
        {
            PlanSwitchSeconds = entity.PlanSwitchSeconds;
        }
    }
}


