using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEmployeeGroupRepository : IGetItemByPredicateRepository<EmployeeGroup>, ICreateItemRepository<EmployeeGroup>, IDeleteItemRepository<EmployeeGroup>
    {
    }
}