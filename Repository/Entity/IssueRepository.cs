using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssueRepository(IGetItemByIdRepository<Issue, int> getItemById,
        ICreateItemRepository<Issue> create,
        IUpsertItemByIdRepository<Issue, int> upsertItemById) : IIssueRepository
    {
        public Task<List<Issue>> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex, CancellationToken ct)
            => getItemById.GetItemsByPredicateAndSortById(predicate: i => i.Id >= startIndex && i.EmployeesUpdatedAt >= dateFrom && i.EmployeesUpdatedAt <= dateTo, asNoTracking: true, ct: ct);

        public Task<Issue?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Issue, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Issue>> GetItemsByPredicateAndSortById(Expression<Func<Issue, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Issue, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Issue item) => create.Create(item);

        public Task Upsert(Issue item, CancellationToken ct = default)
            => upsertItemById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Issue> items, CancellationToken ct = default)
            => upsertItemById.Upsert(items, ct);
    }
}