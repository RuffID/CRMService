using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class ModelRepository(IGetItemByIdRepository<Model, int> getItemById,
        IGetItemByPredicateRepository<Model> getItemByPredicate,
        ICreateItemRepository<Model> create) : IModelRepository
    {
        public Task<Model?> GetItemByPredicateAsync(Expression<Func<Model, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Model>, IQueryable<Model>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Model>> GetItemsByPredicateAsync(Expression<Func<Model, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Model>, IQueryable<Model>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public Task<Model?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Model>, IQueryable<Model>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public void Create(Model item) => create.Create(item);

        public void CreateRange(IEnumerable<Model> entities) => create.CreateRange(entities);
    }
}