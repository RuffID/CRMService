using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeRoleRepository : IGetItemByPredicateRepository<EmployeeRole>, IUpsertItemByPredicateRepository<EmployeeRole>, ICreateItemRepository<EmployeeRole>, IDeleteItemRepository<EmployeeRole>
    {
    }
}