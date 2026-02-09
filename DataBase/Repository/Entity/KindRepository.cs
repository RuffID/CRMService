using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.DataBase.Repository.Entity
{
    public class KindRepository(IGetItemByIdRepository<Kind, int> getItemById,
        IGetItemByPredicateRepository<Kind> getItemByPredicate,
        ICreateItemRepository<Kind> create) : IKindRepository
    {
        public Task<Kind?> GetItemByPredicateAsync(Expression<Func<Kind, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Kind>> GetItemsByPredicateAsync(Expression<Func<Kind, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public Task<Kind?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public void Create(Kind item) => create.Create(item);

        public void CreateRange(IEnumerable<Kind> entities) => create.CreateRange(entities);
    }
}
