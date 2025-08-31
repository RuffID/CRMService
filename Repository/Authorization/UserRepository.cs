using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Repository.Authorization
{
    public class UserRepository(IGetItemByIdRepository<User, Guid> _getById,
        IGetItemByPredicateRepository<User> _getByPredicate,
        ICreateItemRepository<User> _create,
        IUpsertItemByIdRepository<User, Guid> _upsert) : IUserRepository
    {
        public Task<User?> GetItemByPredicate(Expression<Func<User, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<User, object>>[] includes)
            => _getByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<User>> GetItemsByPredicate(Expression<Func<User, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<User, object>>[] includes)
            => _getByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);        
        
        public Task<User?> GetItemById(Guid id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<User, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<User>> GetItemsByPredicateAndSortById(Expression<Func<User, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<User, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(User item) => _create.Create(item);

        public Task Upsert(User item, CancellationToken ct = default)
            => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<User> items, CancellationToken ct = default)
            => _upsert.Upsert(items, ct);        
    }
}
