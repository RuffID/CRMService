using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IEmployeeRepository : IGetItemByIdRepository<Employee, int>, IUpsertItemByIdRepository<Employee, int>, ICreateItemRepository<Employee>
    {
    }
}