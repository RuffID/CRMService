using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EmployeeGroupRepository(IGetItemByPredicateRepository<EmployeeGroup> getItemByPredicate,
        ICreateItemRepository<EmployeeGroup> create,
        IDeleteItemRepository<EmployeeGroup> delete) : IEmployeeGroupRepository
    {
        public Task<EmployeeGroup?> GetItemByPredicateAsync(Expression<Func<EmployeeGroup, bool>> predicate, bool asNoTracking = false, Func<IQueryable<EmployeeGroup>, IQueryable<EmployeeGroup>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<EmployeeGroup>> GetItemsByPredicateAsync(Expression<Func<EmployeeGroup, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<EmployeeGroup>, IQueryable<EmployeeGroup>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(EmployeeGroup item) => create.Create(item);

        public void CreateRange(IEnumerable<EmployeeGroup> entities) => create.CreateRange(entities);

        public void Delete(EmployeeGroup item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<EmployeeGroup> entities) => delete.DeleteRange(entities);
    }
}