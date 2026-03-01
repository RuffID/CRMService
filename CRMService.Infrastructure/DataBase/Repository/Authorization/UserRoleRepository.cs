using CRMService.Application.Abstractions.Database.Repository.Authorization;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Authorization
{
    public class UserRoleRepository(IGetItemByPredicateRepository<UserRole> getItemByPredicate,
        ICreateItemRepository<UserRole> create,
        IDeleteItemRepository<UserRole> delete) : IUserRoleRepository
    {
        public Task<UserRole?> GetItemByPredicateAsync(Expression<Func<UserRole, bool>> predicate, bool asNoTracking = false, Func<IQueryable<UserRole>, IQueryable<UserRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<UserRole>> GetItemsByPredicateAsync(Expression<Func<UserRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<UserRole>, IQueryable<UserRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(UserRole item) => create.Create(item);

        public void CreateRange(IEnumerable<UserRole> entities) => create.CreateRange(entities);

        public void Delete(UserRole item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<UserRole> entities) => delete.DeleteRange(entities);
    }
}



