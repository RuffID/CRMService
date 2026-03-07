using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Domain.Models.CrmEntities;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.CrmEntity
{
    public class PlanColorSchemeRepository(
        IGetItemByIdRepository<PlanColorScheme, Guid, MainContext> getItemById,
        ICreateItemRepository<PlanColorScheme, MainContext> create,
        IGetItemByPredicateRepository<PlanColorScheme, MainContext> getItemByPredicate,
        IDeleteItemRepository<PlanColorScheme, MainContext> delete
    ) : IPlanColorSchemeRepository
    {
        public Task<PlanColorScheme?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<PlanColorScheme>, IQueryable<PlanColorScheme>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<PlanColorScheme?> GetItemByPredicateAsync(Expression<Func<PlanColorScheme, bool>> predicate, bool asNoTracking = false, Func<IQueryable<PlanColorScheme>, IQueryable<PlanColorScheme>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<PlanColorScheme>> GetItemsByPredicateAsync(Expression<Func<PlanColorScheme, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<PlanColorScheme>, IQueryable<PlanColorScheme>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(PlanColorScheme item)
            => create.Create(item);

        public void CreateRange(IEnumerable<PlanColorScheme> entities)
            => create.CreateRange(entities);

        public void Delete(PlanColorScheme item)
            => delete.Delete(item);

        public void DeleteRange(IEnumerable<PlanColorScheme> entities)
            => delete.DeleteRange(entities);
    }
}