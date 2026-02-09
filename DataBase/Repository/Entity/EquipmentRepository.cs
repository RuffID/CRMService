using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EquipmentRepository(IGetItemByIdRepository<Equipment, int> getItemById,
        IGetItemByPredicateRepository<Equipment> getItemByPredicate,
        ICreateItemRepository<Equipment> create) : IEquipmentRepository
    {
        public Task<Equipment?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Equipment?> GetItemByPredicateAsync(Expression<Func<Equipment, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Equipment>> GetItemsByPredicateAsync(Expression<Func<Equipment, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Equipment item) => create.Create(item);

        public void CreateRange(IEnumerable<Equipment> entities) => create.CreateRange(entities);
    }
}
