using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.CrmEntity;
using CRMService.Models.CrmEntities;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.CrmEntity
{
    public class PlanSettingRepository(ICreateItemRepository<PlanSetting> create,
        IGetItemByPredicateRepository<PlanSetting> getItemByPredicate,
        IDeleteItemRepository<PlanSetting> delete) : IPlanSettingRepository
    {
        public Task<PlanSetting?> GetItemByPredicateAsync(Expression<Func<PlanSetting, bool>> predicate, bool asNoTracking = false, Func<IQueryable<PlanSetting>, IQueryable<PlanSetting>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<PlanSetting>> GetItemsByPredicateAsync(Expression<Func<PlanSetting, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<PlanSetting>, IQueryable<PlanSetting>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(PlanSetting item) => create.Create(item);

        public void CreateRange(IEnumerable<PlanSetting> entities) => create.CreateRange(entities);

        public void Delete(PlanSetting item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<PlanSetting> entities) => delete.DeleteRange(entities);
    }
}