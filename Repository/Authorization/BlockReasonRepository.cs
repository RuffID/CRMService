using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Repository.Authorization
{
    public class BlockReasonRepository(IGetItemByIdRepository<BlockReason, Guid> getItemById,
        IGetItemByPredicateRepository<BlockReason> getItemByPredicate,
        ICreateItemRepository<BlockReason> create,
        IUpsertItemByIdRepository<BlockReason, Guid> upsert) : IBlockReasonRepository
    {
        public Task<BlockReason?> GetItemById(Guid id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<BlockReason, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<BlockReason>> GetItemsByPredicateAndSortById(Expression<Func<BlockReason, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<BlockReason, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<BlockReason?> GetItemByPredicate(Expression<Func<BlockReason, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<BlockReason, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<BlockReason>> GetItemsByPredicate(Expression<Func<BlockReason, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<BlockReason, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(BlockReason item) => create.Create(item);

        public Task Upsert(BlockReason item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<BlockReason> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);
    }
}
