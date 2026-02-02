using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueTypeGroupRepository(IGetItemByIdRepository<IssueTypeGroup, int> getItemById,
        IGetItemByPredicateRepository<IssueTypeGroup> getItemByPredicate,
        ICreateItemRepository<IssueTypeGroup> create) : IIssueTypeGroupRepository
    {
        public Task<IssueTypeGroup?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssueTypeGroup>, IQueryable<IssueTypeGroup>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssueTypeGroup?> GetItemByPredicateAsync(Expression<Func<IssueTypeGroup, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssueTypeGroup>, IQueryable<IssueTypeGroup>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssueTypeGroup>> GetItemsByPredicateAsync(Expression<Func<IssueTypeGroup, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssueTypeGroup>, IQueryable<IssueTypeGroup>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(IssueTypeGroup item) => create.Create(item);

        public void CreateRange(IEnumerable<IssueTypeGroup> entities) => create.CreateRange(entities);
    }
}
