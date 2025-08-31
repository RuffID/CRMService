using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Repository.Authorization
{
    public class UserRoleRepository(IGetItemByPredicateRepository<UserRole> getByPredicate,
        ICreateItemRepository<UserRole> create,
        IDeleteItemRepository<UserRole> delete) : IUserRoleRepository
    {
        /*public async Task<IEnumerable<CrmRole>?> GetRolesByUserId(Guid userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.Role != null)
                .Select(ur => new CrmRole
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name
                })
                .ToListAsync();
        }

        public async Task<UserRole?> GetConnectionByUserAndRoleId(UserRole item, bool? trackable = null)
        {
            if (trackable == null || trackable == true)
                return await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == item.UserId && ur.RoleId == item.RoleId);

            return await _context.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.UserId == item.UserId && ur.RoleId == item.RoleId);
        }*/


        public Task<UserRole?> GetItemByPredicate(Expression<Func<UserRole, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<UserRole, object>>[] includes)
            => getByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<UserRole>> GetItemsByPredicate(Expression<Func<UserRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<UserRole, object>>[] includes)
            => getByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(UserRole item) => create.Create(item);

        public void Delete(UserRole item) => delete.Delete(item);
    }
}
