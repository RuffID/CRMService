using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class BlockReasonRepository(CrmAuthorizationContext context, ILoggerFactory logger) : IBlockReasonRepository
    {
        private readonly CrmAuthorizationContext _context = context;
        private readonly ILogger<BlockReasonRepository> _logger = logger.CreateLogger<BlockReasonRepository>();

        public async Task<IEnumerable<BlockReason>?> GetAllItem(Range range)
        {
            try
            {
                return await _context.BlockReasons.AsNoTracking().Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).OrderBy(u => u.BlockingDate).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving block reason list.");
                return null;
            }
        }

        public async Task<BlockReason?> GetItem(BlockReason item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.BlockReasons.FirstOrDefaultAsync(b => b.Id == item.Id);

                return await _context.BlockReasons.AsNoTracking().FirstOrDefaultAsync(b => b.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving block reason.");
                return null;
            }
        }

        public async Task<BlockReason?> GetBlockByUserAndDate(BlockReason block)
        {
            try
            {
                return await _context.BlockReasons.FirstOrDefaultAsync(b => b.UserId == block.UserId && b.BlockingDate == block.BlockingDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving block reason by user id and date.");
                return null;
            }
        }

        public async Task<BlockReason?> GetLastBlockOfUser(BlockReason block)
        {
            try
            {
                return await _context.BlockReasons.FirstOrDefaultAsync(b => b.UserId == block.UserId && b.BlockingDate != null && b.UnblockingDate == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving last block reason.");
                return null;
            }
        }

        public void Update(BlockReason oldItem, BlockReason newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(BlockReason item)
        {
            _context.BlockReasons.Add(item);
        }

        public void Delete(BlockReason item)
        {
            _context.BlockReasons.Remove(item);
        }
    }
}
