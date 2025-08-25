using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Authorization
{
    public partial class BlockReason : IEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime? BlockingDate { get; set; }

        public DateTime? UnblockingDate { get; set; }

        public string? ReasonBlock { get; set; }

        public string? UnblockingReason {  get; set; }

        public virtual User? User { get; set; }

        public void CopyData(BlockReason newItem)
        {
            UserId = newItem.UserId;
            BlockingDate = newItem.BlockingDate;
            UnblockingDate = newItem.UnblockingDate;
            ReasonBlock = newItem.ReasonBlock;
            UnblockingReason = newItem.UnblockingReason;
        }
    }
}