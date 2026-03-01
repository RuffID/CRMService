using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class MaintenanceEntityRepository(IGetItemByIdRepository<MaintenanceEntity, int> getItemById,
        IGetItemByPredicateRepository<MaintenanceEntity> getItemByPredicate,
        ICreateItemRepository<MaintenanceEntity> create) : IMaintenanceEntityRepository
    {
        public Task<MaintenanceEntity?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<MaintenanceEntity>, IQueryable<MaintenanceEntity>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<MaintenanceEntity?> GetItemByPredicateAsync(Expression<Func<MaintenanceEntity, bool>> predicate, bool asNoTracking = false, Func<IQueryable<MaintenanceEntity>, IQueryable<MaintenanceEntity>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<MaintenanceEntity>> GetItemsByPredicateAsync(Expression<Func<MaintenanceEntity, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<MaintenanceEntity>, IQueryable<MaintenanceEntity>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(MaintenanceEntity item) => create.Create(item);

        public void CreateRange(IEnumerable<MaintenanceEntity> entities) => create.CreateRange(entities);
    }
}



