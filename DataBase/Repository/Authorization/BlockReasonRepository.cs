using CRMService.Abstractions.Database.Repository.Authorization;
using CRMService.Models.Authorization;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Authorization
{
    public class BlockReasonRepository(IGetItemByIdRepository<BlockReason, Guid> getItemById,
        IGetItemByPredicateRepository<BlockReason> getItemByPredicate,
        ICreateItemRepository<BlockReason> create) : IBlockReasonRepository
    {
        public Task<BlockReason?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<BlockReason>, IQueryable<BlockReason>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<BlockReason?> GetItemByPredicateAsync(Expression<Func<BlockReason, bool>> predicate, bool asNoTracking = false, Func<IQueryable<BlockReason>, IQueryable<BlockReason>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<BlockReason>> GetItemsByPredicateAsync(Expression<Func<BlockReason, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<BlockReason>, IQueryable<BlockReason>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(BlockReason item) => create.Create(item);

        public void CreateRange(IEnumerable<BlockReason> entities) => create.CreateRange(entities);
    }
}