using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IBlockReasonRepository : ICreateRepository<BlockReason>, IDeleteRepository<BlockReason>, IUpdateRepository<BlockReason>
    {
        Task<IEnumerable<BlockReason>?> GetAllItem(Range range);
        Task<BlockReason?> GetItem(BlockReason item, bool? trackable = null);
        Task<BlockReason?> GetBlockByUserAndDate(BlockReason block);
        Task<BlockReason?> GetLastBlockOfUser(BlockReason block, bool? trackable = null);
    }
}
