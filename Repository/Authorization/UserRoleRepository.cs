using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Repository.Authorization
{
    public class UserRoleRepository(IGetItemByPredicateRepository<UserRole> getItemByPredicate,
        ICreateItemRepository<UserRole> create,
        IDeleteItemRepository<UserRole> delete) : IUserRoleRepository
    {
        public Task<UserRole?> GetItemByPredicate(Expression<Func<UserRole, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<UserRole, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<UserRole>> GetItemsByPredicate(Expression<Func<UserRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<UserRole, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(UserRole item) => create.Create(item);

        public void Delete(UserRole item) => delete.Delete(item);
    }
}
