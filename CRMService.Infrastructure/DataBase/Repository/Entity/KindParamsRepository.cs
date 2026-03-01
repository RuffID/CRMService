using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class KindParamsRepository(IGetItemByPredicateRepository<KindParam> getItemByPredicate,
        ICreateItemRepository<KindParam> create,
        IDeleteItemRepository<KindParam> delete) : IKindParamsRepository
    {    
        public Task<KindParam?> GetItemByPredicateAsync(Expression<Func<KindParam, bool>> predicate, bool asNoTracking = false, Func<IQueryable<KindParam>, IQueryable<KindParam>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<KindParam>> GetItemsByPredicateAsync(Expression<Func<KindParam, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<KindParam>, IQueryable<KindParam>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(KindParam item) => create.Create(item);

        public void CreateRange(IEnumerable<KindParam> entities) => create.CreateRange(entities);

        public void Delete(KindParam item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<KindParam> entities) => delete.DeleteRange(entities);
    }
}



