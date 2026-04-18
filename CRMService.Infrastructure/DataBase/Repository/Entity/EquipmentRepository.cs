using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Contracts.Models.Request;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class EquipmentRepository(
        IGetItemByIdRepository<Equipment, int, MainContext> getItemById,
        IGetItemByPredicateRepository<Equipment, MainContext> getItemByPredicate,
        ICreateItemRepository<Equipment, MainContext> create,
        IQueryRepository<Equipment, MainContext> query
    ) : IEquipmentRepository
    {
        public Task<Equipment?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Equipment?> GetItemByPredicateAsync(Expression<Func<Equipment, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Equipment>> GetItemsByPredicateAsync(Expression<Func<Equipment, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Equipment item)
            => create.Create(item);

        public void CreateRange(IEnumerable<Equipment> entities)
            => create.CreateRange(entities);

        public async Task<int> GetCountByFilterAsync(EquipmentListRequest request, int? maxCount, CancellationToken ct)
        {
            IQueryable<Equipment> items = ApplyFilters(query.Query(asNoTracking: true), request);

            if (maxCount.HasValue)
                return await items.Take(maxCount.Value).CountAsync(ct);

            return await items.CountAsync(ct);
        }

        public Task<List<Equipment>> GetPageByFilterAsync(EquipmentListRequest request, int skip, int take, CancellationToken ct)
        {
            IQueryable<Equipment> items = ApplyFilters(query.Query(asNoTracking: true), request)
                .Include(equipment => equipment.Kind)
                .Include(equipment => equipment.Manufacturer)
                .Include(equipment => equipment.Model)
                .Include(equipment => equipment.Company)
                .ThenInclude(company => company!.Category)
                .Include(equipment => equipment.MaintenanceEntities)
                .Include(equipment => equipment.Parameters)
                .ThenInclude(parameter => parameter.KindParameter)
                .OrderByDescending(equipment => equipment.Id)
                .Skip(skip)
                .Take(take);

            return items.ToListAsync(ct);
        }

        private static IQueryable<Equipment> ApplyFilters(IQueryable<Equipment> query, EquipmentListRequest request)
        {
            if (request.EquipmentId.HasValue)
                query = query.Where(equipment => equipment.Id == request.EquipmentId.Value);

            if (request.CompanyIds != null && request.CompanyIds.Count > 0)
                query = query.Where(equipment => equipment.CompanyId.HasValue && request.CompanyIds.Contains(equipment.CompanyId.Value));

            if (request.MaintenanceEntityIds != null && request.MaintenanceEntityIds.Count > 0)
                query = query.Where(equipment => equipment.MaintenanceEntitiesId.HasValue && request.MaintenanceEntityIds.Contains(equipment.MaintenanceEntitiesId.Value));

            return query;
        }
    }
}
