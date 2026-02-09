using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEmployeeRoleRepository : IGetItemByPredicateRepository<EmployeeRole>, ICreateItemRepository<EmployeeRole>, IDeleteItemRepository<EmployeeRole>
    {
    }
}