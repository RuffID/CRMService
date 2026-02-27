using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EmployeeRepository(IGetItemByIdRepository<Employee, int> getItemByid,
        IGetItemByPredicateRepository<Employee> getItemByPredicate,
        ICreateItemRepository<Employee> create) : IEmployeeRepository
    {
        public Task<Employee?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Employee>, IQueryable<Employee>>? include = null, CancellationToken ct = default)
            => getItemByid.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Employee?> GetItemByPredicateAsync(Expression<Func<Employee, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Employee>, IQueryable<Employee>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Employee>> GetItemsByPredicateAsync(Expression<Func<Employee, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Employee>, IQueryable<Employee>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Employee item) => create.Create(item);

        public void CreateRange(IEnumerable<Employee> entities) => create.CreateRange(entities);
    }
}
