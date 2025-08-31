using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeRepository : IGetItemByIdRepository<Employee, int>, IUpsertItemByIdRepository<Employee, int>, ICreateItemRepository<Employee>, ICountItemRepository<Employee>
    {
    }
}