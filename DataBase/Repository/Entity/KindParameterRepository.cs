using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class KindParameterRepository(IGetItemByIdRepository<KindsParameter, int> getItemById,
        IGetItemByPredicateRepository<KindsParameter> getItemByPredicate,
        ICreateItemRepository<KindsParameter> create) : IKindParameterRepository
    {
        public Task<KindsParameter?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<KindsParameter?> GetItemByPredicateAsync(Expression<Func<KindsParameter, bool>> predicate, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<KindsParameter>> GetItemsByPredicateAsync(Expression<Func<KindsParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(KindsParameter item) => create.Create(item);

        public void CreateRange(IEnumerable<KindsParameter> entities) => create.CreateRange(entities);
    }
}