using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskManufacturerRepository(
        IGetItemByIdRepository<Manufacturer, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Manufacturer, OkdeskContext> getItemByPredicate) : IOkdeskManufacturerRepository
    {
        public Task<Manufacturer?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Manufacturer?> GetItemByPredicateAsync(Expression<Func<Manufacturer, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Manufacturer>> GetItemsByPredicateAsync(Expression<Func<Manufacturer, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Manufacturer>, IQueryable<Manufacturer>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}