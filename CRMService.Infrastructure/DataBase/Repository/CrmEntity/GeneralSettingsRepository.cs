using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Domain.Models.CrmEntities;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.CrmEntity
{
    public class GeneralSettingsRepository(
        IGetItemByIdRepository<GeneralSettings, Guid, MainContext> getItemById,
        ICreateItemRepository<GeneralSettings, MainContext> create,
        IGetItemByPredicateRepository<GeneralSettings, MainContext> getItemByPredicate,
        IDeleteItemRepository<GeneralSettings, MainContext> delete
    ) : IGeneralSettingsRepository
    {
        public Task<GeneralSettings?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<GeneralSettings>, IQueryable<GeneralSettings>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<GeneralSettings?> GetItemByPredicateAsync(Expression<Func<GeneralSettings, bool>> predicate, bool asNoTracking = false, Func<IQueryable<GeneralSettings>, IQueryable<GeneralSettings>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<GeneralSettings>> GetItemsByPredicateAsync(Expression<Func<GeneralSettings, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<GeneralSettings>, IQueryable<GeneralSettings>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(GeneralSettings item)
            => create.Create(item);

        public void CreateRange(IEnumerable<GeneralSettings> entities)
            => create.CreateRange(entities);

        public void Delete(GeneralSettings item)
            => delete.Delete(item);

        public void DeleteRange(IEnumerable<GeneralSettings> entities)
            => delete.DeleteRange(entities);
    }
}