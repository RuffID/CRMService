using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueTypeRepository(IGetItemByIdRepository<IssueType, int> getItemById,
        IGetItemByPredicateRepository<IssueType> getItemByPredicate,
        ICreateItemRepository<IssueType> create) : IIssueTypeRepository
    {
        public Task<IssueType?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssueType?> GetItemByPredicateAsync(Expression<Func<IssueType, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssueType>> GetItemsByPredicateAsync(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssueType>, IQueryable<IssueType>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(IssueType item) => create.Create(item);

        public void CreateRange(IEnumerable<IssueType> entities) => create.CreateRange(entities);
    }
}
