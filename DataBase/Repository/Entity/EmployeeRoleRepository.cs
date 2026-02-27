using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;

namespace CRMService.DataBase.Repository.Entity
{
    public class EmployeeRoleRepository(IGetItemByPredicateRepository<EmployeeRole> getItemByPredicate,
        ICreateItemRepository<EmployeeRole> create,
        IDeleteItemRepository<EmployeeRole> delete) : IEmployeeRoleRepository
    {
        public Task<EmployeeRole?> GetItemByPredicateAsync(Expression<Func<EmployeeRole, bool>> predicate, bool asNoTracking = false, Func<IQueryable<EmployeeRole>, IQueryable<EmployeeRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<EmployeeRole>> GetItemsByPredicateAsync(Expression<Func<EmployeeRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<EmployeeRole>, IQueryable<EmployeeRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(EmployeeRole item) => create.Create(item);

        public void CreateRange(IEnumerable<EmployeeRole> entities) => create.CreateRange(entities);

        public void Delete(EmployeeRole item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<EmployeeRole> entities) => delete.DeleteRange(entities);
    }
}
