using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.DataBase.Repository.Entity
{
    public class ManufacturerRepository(IGetItemByIdRepository<Manufacturer, int> getItemById,
        IGetItemByPredicateRepository<Manufacturer> getItemByPredicate,
        ICreateItemRepository<Manufacturer> create) : IManufacturerRepository
    {
        public Task<Manufacturer?> GetItemByPredicateAsync(Expression<Func<Manufacturer, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Manufacturer>> GetItemsByPredicateAsync(Expression<Func<Manufacturer, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public Task<Manufacturer?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public void Create(Manufacturer item) => create.Create(item);

        public void CreateRange(IEnumerable<Manufacturer> entities) => create.CreateRange(entities);
    }
}
