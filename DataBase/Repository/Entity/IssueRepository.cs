using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueRepository(IGetItemByIdRepository<Issue, int> getItemById,
        IGetItemByPredicateRepository<Issue> getItemByPredicate,
        ICreateItemRepository<Issue> create) : IIssueRepository
    {
        public Task<Issue?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Issue?> GetItemByPredicateAsync(Expression<Func<Issue, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Issue>> GetItemsByPredicateAsync(Expression<Func<Issue, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Issue>, IQueryable<Issue>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
        
        public void Create(Issue item) => create.Create(item);

        public void CreateRange(IEnumerable<Issue> entities) => create.CreateRange(entities);
    }
}