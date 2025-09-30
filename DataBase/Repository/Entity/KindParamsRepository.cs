using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class KindParamsRepository(IGetItemByPredicateRepository<KindParam> getItemByPredicate,
        ICreateItemRepository<KindParam> create,
        IDeleteItemRepository<KindParam> delete) : IKindParamsRepository
    {    
        public Task<KindParam?> GetItemByPredicate(Expression<Func<KindParam, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindParam, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<KindParam>> GetItemsByPredicate(Expression<Func<KindParam, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindParam, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(KindParam item) => create.Create(item);

        public void Delete(KindParam item) => delete.Delete(item);
    }
}
