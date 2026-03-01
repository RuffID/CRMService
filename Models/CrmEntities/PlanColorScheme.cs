using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Models.CrmEntities
{
    public class PlanColorScheme : IEntity<Guid>, ICopyable<PlanColorScheme>
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public int FromPercent { get; set; }
        public int? ToPercent { get; set; }
        public string Color { get; set; } = string.Empty;

        public virtual Plan Plan { get; set; } = null!;

        public void CopyData(PlanColorScheme entity)
        {
            PlanId = entity.PlanId;
            FromPercent = entity.FromPercent;
            ToPercent = entity.ToPercent;
            Color = entity.Color;
        }
    }
}