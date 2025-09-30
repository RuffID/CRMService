using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IEmployeeRoleRepository : IGetItemByPredicateRepository<EmployeeRole>, IUpsertItemByPredicateRepository<EmployeeRole>, ICreateItemRepository<EmployeeRole>, IDeleteItemRepository<EmployeeRole>
    {
    }
}