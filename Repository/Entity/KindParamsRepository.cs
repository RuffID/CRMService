using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class KindParamsRepository(IGetItemByPredicateRepository<KindParam> _getByPredicate,
        ICreateItemRepository<KindParam> _create,
        IDeleteItemRepository<KindParam> _delete) : IKindParamsRepository
    {    
        public Task<KindParam?> GetItemByPredicate(Expression<Func<KindParam, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindParam, object>>[] includes)
            => _getByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<KindParam>> GetItemsByPredicate(Expression<Func<KindParam, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindParam, object>>[] includes)
            => _getByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(KindParam item) => _create.Create(item);

        public void Delete(KindParam item) => _delete.Delete(item);
    }
}
