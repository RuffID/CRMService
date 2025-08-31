using CRMService.Interfaces.Entity;

namespace CRMService.Models.Authorization
{
    public partial class BlockReason : IEntity<Guid>, ICopyable<BlockReason>
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime BlockingDate { get; set; }

        public DateTime? UnblockingDate { get; set; }

        public string ReasonBlock { get; set; } = string.Empty;

        public string? UnblockingReason {  get; set; }

        public virtual User User { get; set; } = null!;

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