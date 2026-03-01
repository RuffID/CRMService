using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.CrmEntity;
using CRMService.Models.CrmEntities;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.CrmEntity
{
    public class PlanRepository(IGetItemByIdRepository<Plan, Guid> getItemById,
        ICreateItemRepository<Plan> create,
        IGetItemByPredicateRepository<Plan> getItemByPredicate,
        IDeleteItemRepository<Plan> delete) : IPlanRepository
    {
        public Task<Plan?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<Plan>, IQueryable<Plan>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Plan?> GetItemByPredicateAsync(Expression<Func<Plan, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Plan>, IQueryable<Plan>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Plan>> GetItemsByPredicateAsync(Expression<Func<Plan, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Plan>, IQueryable<Plan>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Plan item) => create.Create(item);

        public void CreateRange(IEnumerable<Plan> entities) => create.CreateRange(entities);

        public void Delete(Plan item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<Plan> entities) => delete.DeleteRange(entities);
    }
}