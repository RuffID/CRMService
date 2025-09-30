using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueTypeRepository(IGetItemByIdRepository<IssueType, int> getItemById,
        IGetItemByPredicateRepository<IssueType> getItemByPredicate,
        ICreateItemRepository<IssueType> create,
        IUpsertItemByIdRepository<IssueType, int> upsert) : IIssueTypeRepository
    {
        public Task<IssueType?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueType>> GetItemsByPredicateAndSortById(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
        => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueType?> GetItemByPredicate(Expression<Func<IssueType, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<IssueType>> GetItemsByPredicate(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(IssueType item) => create.Create(item);

        public Task Upsert(IssueType item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<IssueType> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);
    }
}
