using CRMService.Application.Abstractions.Database.Repository.Authorization;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Authorization
{
    public class UserRepository(IGetItemByIdRepository<User, Guid> getItemById,
        IGetItemByPredicateRepository<User> getItemByPredicate,
        ICreateItemRepository<User> create) : IUserRepository
    {
        public Task<User?> GetItemByPredicateAsync(Expression<Func<User, bool>> predicate, bool asNoTracking = false, Func<IQueryable<User>, IQueryable<User>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<User>> GetItemsByPredicateAsync(Expression<Func<User, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<User>, IQueryable<User>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);        
        
        public Task<User?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<User>, IQueryable<User>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public void Create(User item) => create.Create(item);

        public void CreateRange(IEnumerable<User> entities) => create.CreateRange(entities);
    }
}



