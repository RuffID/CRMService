using CRMService.Interfaces.Entity;

namespace CRMService.Models.Dto.Authorization
{
    public partial class BlockReasonDto : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime? BlockingDate { get; set; }

        public DateTime? UnblockingDate { get; set; }

        public string? ReasonBlock { get; set; }

        public string? UnblockingReason { get; set; }
    }
}