using CRMService.Abstractions.Entity;

namespace CRMService.Models.CrmEntities
{
    public class PlanColorScheme : IEntity<Guid>, ICopyable<PlanColorScheme>
    {
        public Guid Id { get; set; }
        public int FromPercent { get; set; }
        public int ToPercent { get; set; }
        public string Color { get; set; } = string.Empty;

        public void CopyData(PlanColorScheme entity)
        {
            FromPercent = entity.FromPercent;
            ToPercent = entity.ToPercent;
            Color = entity.Color;
        }
    }
}
