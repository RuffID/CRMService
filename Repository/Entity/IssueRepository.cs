using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssueRepository(IGetItemByIdRepository<Issue, int> _getById,
        ICreateItemRepository<Issue> _create,
        IUpsertItemByIdRepository<Issue, int> _upsert) : IIssueRepository
    {
        public Task<List<Issue>> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex, CancellationToken ct)
            => _getById.GetItemsByPredicateAndSortById(predicate: i => i.Id >= startIndex && i.EmployeesUpdatedAt >= dateFrom && i.EmployeesUpdatedAt <= dateTo, asNoTracking: true, ct: ct);

        public Task<Issue?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Issue, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Issue>> GetItemsByPredicateAndSortById(Expression<Func<Issue, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Issue, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Issue item) => _create.Create(item);

        public Task Upsert(Issue item, CancellationToken ct = default)
            => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<Issue> items, CancellationToken ct = default)
            => _upsert.Upsert(items, ct);
    }
}