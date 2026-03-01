using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class ParameterRepository(IGetItemByPredicateRepository<EquipmentParameter> getItemByPredicate,
        ICreateItemRepository<EquipmentParameter> create) : IParameterRepository
    {
        public Task<EquipmentParameter?> GetItemByPredicateAsync(Expression<Func<EquipmentParameter, bool>> predicate, bool asNoTracking = false, Func<IQueryable<EquipmentParameter>, IQueryable<EquipmentParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<EquipmentParameter>> GetItemsByPredicateAsync(Expression<Func<EquipmentParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<EquipmentParameter>, IQueryable<EquipmentParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(EquipmentParameter item) => create.Create(item);

        public void CreateRange(IEnumerable<EquipmentParameter> entities) => create.CreateRange(entities);
    }
}



