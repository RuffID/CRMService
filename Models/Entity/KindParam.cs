using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class KindParam : IEntity
    {
        public int Id { get; set; }

        public int? KindId { get; set; }

        public int? KindParameterId { get; set; }

        public virtual Kind? Kind { get; set; }

        public virtual KindsParameter? KindParameter { get; set; }
    }
}