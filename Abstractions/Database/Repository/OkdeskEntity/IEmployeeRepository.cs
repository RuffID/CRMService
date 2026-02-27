using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEmployeeRepository : IGetItemByIdRepository<Employee, int>, IGetItemByPredicateRepository<Employee>, ICreateItemRepository<Employee>
    {
    }
}