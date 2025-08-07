using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IBlockReasonRepository : ICreateRepository<BlockReason>, IDeleteRepository<BlockReason>, IUpdateRepository<BlockReason>
    {
        public Task<BlockReason?> GetBlockByUserAndDate(BlockReason block);
        public Task<BlockReason?> GetLastBlockOfUser(BlockReason block);
    }
}
