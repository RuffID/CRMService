using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IEmployeeGroupRepository : IGetItemByPredicateRepository<EmployeeGroup>, ICreateItemRepository<EmployeeGroup>, IDeleteItemRepository<EmployeeGroup>
    {
    }
}